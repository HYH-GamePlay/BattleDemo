using System;
using System.Collections.Generic;
using Battle.CombatInfo;

namespace Battle.Core
{
    internal sealed class CombatInfoStore
    {
        private readonly Dictionary<InfoHandle, IBehaviorInfo> _infos = new Dictionary<InfoHandle, IBehaviorInfo>();
        private readonly Dictionary<IBehaviorInfo, InfoHandle> _handles = new Dictionary<IBehaviorInfo, InfoHandle>();
        private readonly Dictionary<InfoHandle, uint> _infoVersions = new Dictionary<InfoHandle, uint>();
        private int _nextInfoHandle = 1;

        public InfoHandle AddInfo<T>(T info) where T : class, IBehaviorInfo
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (_handles.ContainsKey(info))
            {
                throw new InvalidOperationException($"Info already exists: {info.GetType().Name}");
            }

            var handle = new InfoHandle(_nextInfoHandle++);
            _infos.Add(handle, info);
            _handles.Add(info, handle);
            _infoVersions.Add(handle, 0);
            return handle;
        }

        public bool RemoveInfo(InfoHandle handle)
        {
            if (!_infos.TryGetValue(handle, out var info))
            {
                return false;
            }

            _handles.Remove(info);
            _infoVersions.Remove(handle);
            return _infos.Remove(handle);
        }

        public bool RemoveInfo(IBehaviorInfo info)
        {
            if (info == null || !_handles.TryGetValue(info, out var handle))
            {
                return false;
            }

            return RemoveInfo(handle);
        }

        public bool HasInfo(InfoHandle handle)
        {
            return _infos.ContainsKey(handle);
        }

        public bool HasInfo(IBehaviorInfo info)
        {
            return info != null && _handles.ContainsKey(info);
        }

        public InfoHandle GetHandle(IBehaviorInfo info)
        {
            if (info != null && _handles.TryGetValue(info, out var handle))
            {
                return handle;
            }

            throw new InvalidOperationException("Info does not exist.");
        }

        public T GetInfo<T>(InfoHandle handle) where T : class, IBehaviorInfo
        {
            if (TryGetInfo<T>(handle, out var info))
            {
                return info;
            }

            throw new InvalidOperationException($"Info does not exist or type mismatch. Handle: {handle}, Info: {typeof(T).Name}");
        }

        public bool TryGetInfo<T>(InfoHandle handle, out T info) where T : class, IBehaviorInfo
        {
            info = null;
            if (!_infos.TryGetValue(handle, out var value))
            {
                return false;
            }

            info = value as T;
            return info != null;
        }

        public uint GetVersion(InfoHandle handle)
        {
            return _infoVersions.TryGetValue(handle, out var version) ? version : 0;
        }

        public void MarkDirty(InfoHandle handle)
        {
            if (!_infoVersions.ContainsKey(handle))
            {
                throw new InvalidOperationException($"Info does not exist: {handle}");
            }

            _infoVersions[handle]++;
        }

        public void MarkDirty(IBehaviorInfo info)
        {
            MarkDirty(GetHandle(info));
        }
    }
}
