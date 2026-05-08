using System;
using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    public static class ActionDataSerialize
    {
        public static ActionRawData ToRaw(ActionData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var count = data.instructs != null ? data.instructs.Count : 0;
            var raw = new ActionRawData
            {
                length = data.GetLength(),
                begin = new uint[count],
                end = new uint[count],
                instructTypes = new ushort[count],
                instructData = new byte[count][],
            };

            for (var i = 0; i < count; i++)
            {
                var instruct = data.instructs[i];
                if (instruct == null || instruct.data == null)
                {
                    continue;
                }

                raw.begin[i] = instruct.begin;
                raw.end[i] = instruct.end;
                raw.instructTypes[i] = (ushort)instruct.data.id;
                raw.instructData[i] = SerializeInstructData(instruct.data);
            }

            return raw;
        }

        public static ActionData FromRaw(ActionRawData raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException(nameof(raw));
            }

            var count = raw.instructTypes != null ? raw.instructTypes.Length : 0;
            var data = new ActionData
            {
                length = raw.length,
                instructs = new List<Instruct>(count),
            };

            for (var i = 0; i < count; i++)
            {
                var type = (InstructType)raw.instructTypes[i];
                var bytes = raw.instructData != null && i < raw.instructData.Length ? raw.instructData[i] : null;
                data.instructs.Add(new Instruct
                {
                    begin = raw.begin != null && i < raw.begin.Length ? raw.begin[i] : 0,
                    end = raw.end != null && i < raw.end.Length ? raw.end[i] : 0,
                    data = DeserializeInstructData(type, bytes),
                });
            }

            return data;
        }

        public static byte[] Serialize(ActionData data)
        {
            return MessagePackSerializer.Serialize(ToRaw(data));
        }

        public static ActionData Deserialize(byte[] bytes)
        {
            var raw = MessagePackSerializer.Deserialize<ActionRawData>(bytes);
            return FromRaw(raw);
        }

        private static byte[] SerializeInstructData(InstructData data)
        {
            switch (data)
            {
                case AnimationData typed:
                    return MessagePackSerializer.Serialize(typed);
                case AddTagData typed:
                    return MessagePackSerializer.Serialize(typed);
                case RemoveTagData typed:
                    return MessagePackSerializer.Serialize(typed);
                case CollisionData typed:
                    return MessagePackSerializer.Serialize(typed);
                case ApplyEffectData typed:
                    return MessagePackSerializer.Serialize(typed);
                case MotionData typed:
                    return MessagePackSerializer.Serialize(typed);
                case CreateBulletData typed:
                    return MessagePackSerializer.Serialize(typed);
                case AudioData typed:
                    return MessagePackSerializer.Serialize(typed);
                case CameraData typed:
                    return MessagePackSerializer.Serialize(typed);
                case VfxData typed:
                    return MessagePackSerializer.Serialize(typed);
                case ActionLinkData typed:
                    return MessagePackSerializer.Serialize(typed);
                default:
                    throw new NotSupportedException($"Unsupported instruct data type: {data.GetType().Name}");
            }
        }

        private static InstructData DeserializeInstructData(InstructType type, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            switch (type)
            {
                case InstructType.Animation:
                    return MessagePackSerializer.Deserialize<AnimationData>(bytes);
                case InstructType.AddTag:
                    return MessagePackSerializer.Deserialize<AddTagData>(bytes);
                case InstructType.RemoveTag:
                    return MessagePackSerializer.Deserialize<RemoveTagData>(bytes);
                case InstructType.Collision:
                    return MessagePackSerializer.Deserialize<CollisionData>(bytes);
                case InstructType.ApplyEffect:
                    return MessagePackSerializer.Deserialize<ApplyEffectData>(bytes);
                case InstructType.Motion:
                    return MessagePackSerializer.Deserialize<MotionData>(bytes);
                case InstructType.CreateBullet:
                    return MessagePackSerializer.Deserialize<CreateBulletData>(bytes);
                case InstructType.Audio:
                    return MessagePackSerializer.Deserialize<AudioData>(bytes);
                case InstructType.Camera:
                    return MessagePackSerializer.Deserialize<CameraData>(bytes);
                case InstructType.Vfx:
                    return MessagePackSerializer.Deserialize<VfxData>(bytes);
                case InstructType.ActionLink:
                    return MessagePackSerializer.Deserialize<ActionLinkData>(bytes);
                default:
                    throw new NotSupportedException($"Unsupported instruct type: {type}");
            }
        }
    }
}
