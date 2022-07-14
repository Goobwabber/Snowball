using LiteNetLib.Utils;
using MultiplayerCore.Networking.Abstractions;
using UnityEngine;

namespace Snowball.Networking
{
    public class SnowballPacket : MpPacket
    {
        public Vector3Serializable position { get; set; }
        public QuaternionSerializable rotation { get; set; }

        public override void Serialize(NetDataWriter writer)
        {
            position.Serialize(writer);
            rotation.Serialize(writer);
        }

        public override void Deserialize(NetDataReader reader)
        {
            position = new(reader);
            rotation = new(reader);
        }
    }
}