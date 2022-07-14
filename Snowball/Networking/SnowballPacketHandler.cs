using MultiplayerCore.Networking;
using System;
using Zenject;

namespace Snowball.Networking
{
    public class SnowballPacketHandler : IInitializable, IDisposable
    {
        public event Action<SnowballPacket, IConnectedPlayer> SnowballPacketReceived = null!;

        private readonly MpPacketSerializer _packetSerializer;

        [Inject]
        internal SnowballPacketHandler(
            MpPacketSerializer packetSerializer)
        {
            _packetSerializer = packetSerializer;
        }

        public void Initialize()
        {
            _packetSerializer.RegisterCallback<SnowballPacket>(HandlePacket);
        }

        public void Dispose()
        {
            _packetSerializer.UnregisterCallback<SnowballPacket>();
        }

        public void HandlePacket(SnowballPacket packet, IConnectedPlayer player)
        {
            SnowballPacketReceived?.Invoke(packet, player);
        }
    }
}
