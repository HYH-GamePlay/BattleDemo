using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Combat.View
{
    /// <summary>
    /// Transform 批处理系统 — 用 IJobParallelForTransform + Burst 将逻辑坐标批量写入 Unity Transform。
    /// 在 LateUpdate 执行，确保逻辑层 Tick 已完成。
    /// </summary>
    public class TransformSyncSystem : MonoBehaviour
    {
        private readonly List<Core.CombatEntity> _entities = new();
        private readonly List<Transform> _transforms = new();

        private TransformAccessArray _transformAccessArray;
        private NativeArray<float3> _positions;
        private bool _dirty;

        public void Register(Core.CombatEntity entity, Transform t)
        {
            _entities.Add(entity);
            _transforms.Add(t);
            RebuildArrays();
        }

        public void Unregister(Core.CombatEntity entity)
        {
            int idx = _entities.IndexOf(entity);
            if (idx < 0) return;
            _entities.RemoveAt(idx);
            _transforms.RemoveAt(idx);
            RebuildArrays();
        }

        private void LateUpdate()
        {
            if (_entities.Count == 0) return;

            // 从逻辑层收集坐标
            for (int i = 0; i < _entities.Count; i++)
                _positions[i] = new float3(_entities[i].Position.X, 0f, _entities[i].Position.Z);

            var handle = new SyncTransformJob { Positions = _positions }
                .Schedule(_transformAccessArray);
            handle.Complete();
        }

        private void RebuildArrays()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_positions.IsCreated) _positions.Dispose();

            if (_transforms.Count == 0) return;
            _transformAccessArray = new TransformAccessArray(_transforms.ToArray());
            _positions = new NativeArray<float3>(_transforms.Count, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_positions.IsCreated) _positions.Dispose();
        }

        [BurstCompile]
        private struct SyncTransformJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<float3> Positions;
            public void Execute(int index, TransformAccess transform)
                => transform.position = Positions[index];
        }
    }
}
