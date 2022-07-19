using LiteNetLib.Utils;
using MultiplayerCore.Networking.Abstractions;
using System;
using System.Text;

namespace Snowball.Networking
{
    internal class SnowballReleasePacket : MpPacket
    {
        public Guid id { get; set; }
        public Vector3Serializable position { get; set; }
        public QuaternionSerializable rotation { get; set; }
        public Vector3Serializable velocity { get; set; }
        public Vector3Serializable angular { get; set; }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(Encoding.UTF8.GetBytes(id.ToString()));
            position.Serialize(writer);
            rotation.Serialize(writer);
            velocity.Serialize(writer);
            angular.Serialize(writer);
        }

        public override void Deserialize(NetDataReader reader)
        {
            byte[] guid = new byte[36];
            reader.GetBytes(guid, 36);
            id = new Guid(Encoding.UTF8.GetString(guid));
            position = new(reader);
            rotation = new(reader);
            velocity = new(reader);
            angular = new(reader);
        }
    }
}
