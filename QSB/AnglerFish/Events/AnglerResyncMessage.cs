using System;
using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.AnglerFish.Events {
    public class AnglerResyncMessage : WorldObjectMessage {
        public enum Type : byte {
            /// tell host we entered/left a sector
            SectorEnterLeave,
            /// host asks player for angler transform
            TransformRequest,
            /// player broadcasts with that transform
            TransformBroadcast
        }

        public static AnglerResyncMessage SectorEnterLeave(int anglerId, string sector, bool entered) =>
            new AnglerResyncMessage {
                OnlySendToHost = true,
                type = Type.SectorEnterLeave,
                ObjectId = anglerId,
                sector = sector,
                entered = entered
            };

        public static AnglerResyncMessage TransformRequest(int anglerId, uint toId) =>
            new AnglerResyncMessage {
                type = Type.TransformRequest,
                ObjectId = anglerId,
                AboutId = toId
            };

        public static AnglerResyncMessage TransformBroadcast(int anglerId, uint fromId, Vector3 pos, Quaternion rot) =>
            new AnglerResyncMessage {
                type = Type.TransformBroadcast,
                ObjectId = anglerId,
                AboutId = fromId,
                pos = pos,
                rot = rot,
            };

        public Type type;

        public string sector;
        public bool entered;

        public Vector3 pos;
        public Quaternion rot;

        public override void Deserialize(QNetworkReader reader) {
            base.Deserialize(reader);
            // use object id for angler
            type = (Type)reader.ReadByte();
            switch (type) {
                case Type.SectorEnterLeave:
                    sector = reader.ReadString();
                    entered = reader.ReadBoolean();
                    break;
                case Type.TransformRequest:
                    // use about id to request specific player
                    break;
                case Type.TransformBroadcast:
                    // use about id to represent what player has this transform
                    pos = reader.ReadVector3();
                    rot = reader.ReadQuaternion();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override void Serialize(QNetworkWriter writer) {
            base.Serialize(writer);
            // use object id for angler
            writer.Write((byte)type);
            switch (type) {
                case Type.SectorEnterLeave:
                    writer.Write(sector);
                    writer.Write(entered);
                    break;
                case Type.TransformRequest:
                    // use about id to request specific player
                    break;
                case Type.TransformBroadcast:
                    // use about id to represent what player has this transform
                    writer.Write(pos);
                    writer.Write(rot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override string ToString() {
            switch (type) {
                case Type.SectorEnterLeave:
                    return $"{FromId} {(entered ? "entered" : "exited")} {sector} ({ObjectId})";
                case Type.TransformRequest:
                    return $"{AboutId}: {ObjectId} trans?";
                case Type.TransformBroadcast:
                    return $"{AboutId}: {ObjectId} trans = {pos} {rot}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
