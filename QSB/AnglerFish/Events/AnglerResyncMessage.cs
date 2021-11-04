using System;
using QSB.WorldSync.Events;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.AnglerFish.Events {
    public class AnglerResyncMessage : WorldObjectMessage {
        public enum Type : byte {
            /// tell host we are in/out of a sector
            OccupantUpdate,
            /// host asks player for angler transform
            TransformRequest,
            /// player broadcasts with that transform
            TransformBroadcast
        }

        public Type type;

        public string sector;
        public bool inside;

        public Vector3 pos;
        public Quaternion rot;

        public override void Deserialize(QNetworkReader reader) {
            base.Deserialize(reader);
            // use object id for angler
            type = (Type)reader.ReadByte();
            switch (type) {
                case Type.OccupantUpdate:
                    sector = reader.ReadString();
                    inside = reader.ReadBoolean();
                    break;
                case Type.TransformRequest:
                    // use about id to request specific player
                    break;
                case Type.TransformBroadcast:
                    pos = reader.ReadVector3();
                    rot = reader.ReadQuaternion();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override void Serialize(QNetworkWriter writer) {
            if (type == Type.OccupantUpdate) OnlySendToHost = true;

            base.Serialize(writer);
            // use object id for angler
            writer.Write((byte)type);
            switch (type) {
                case Type.OccupantUpdate:
                    writer.Write(sector);
                    writer.Write(inside);
                    break;
                case Type.TransformRequest:
                    // use about id to request specific player
                    break;
                case Type.TransformBroadcast:
                    writer.Write(pos);
                    writer.Write(rot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override string ToString() {
            switch (type) {
                case Type.OccupantUpdate:
                    return $"angler {ObjectId}: {FromId} in {sector} = {inside}";
                case Type.TransformRequest:
                    return $"angler {ObjectId}: trans req -> {AboutId}";
                case Type.TransformBroadcast:
                    return $"angler {ObjectId}: trans bc from {FromId} = {pos} {rot}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
