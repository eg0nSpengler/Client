// #### AUTO-GENERATED CODE ####
// Please avoid editing
// Copyright © Brisk Technologies

namespace Brisk.Serialization {
    public sealed class AutoGenerated_BriskSerialization : Brisk.Serialization.Serializer {
        public override void SerializeReliable<T>(T obj, Lidgren.Network.NetOutgoingMessage msg) {
            switch (obj) {
            case TestSync testsync:
                msg.Write(testsync.A);
                msg.Write(testsync.B);
                break;
            }
        }
        public override void DeserializeReliable<T>(T obj, Lidgren.Network.NetIncomingMessage msg) {
            switch (obj) {
            case TestSync testsync:
                testsync.A = msg.ReadBoolean();
                testsync.B = msg.ReadBoolean();
                break;
            }
        }
        public override void SerializeUnreliable<T>(T obj, Lidgren.Network.NetOutgoingMessage msg) {
            switch (obj) {
            case Brisk.Entities.NetEntity netentity:
                msg.Write(netentity.Position.x);
                msg.Write(netentity.Position.y);
                msg.Write(netentity.Position.z);
                msg.Write(netentity.Rotation.x);
                msg.Write(netentity.Rotation.y);
                msg.Write(netentity.Rotation.z);
                msg.Write(netentity.Scale.x);
                msg.Write(netentity.Scale.y);
                msg.Write(netentity.Scale.z);
                break;
            }
        }
        public override void DeserializeUnreliable<T>(T obj, Lidgren.Network.NetIncomingMessage msg) {
            switch (obj) {
            case Brisk.Entities.NetEntity netentity:
                netentity.Position = new UnityEngine.Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
                netentity.Rotation = new UnityEngine.Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
                netentity.Scale = new UnityEngine.Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
                break;
            }
        }
    }
}
