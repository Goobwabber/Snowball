using Snowball.Networking;
using UnityEngine;
using Zenject;

namespace Snowball.Objects
{
    public class ConnectedSnowball : MonoBehaviour
    {
        private Rigidbody _rigidbody = null!;
        private IConnectedPlayer _player = null!;
        private SnowballPacketHandler _packetHandler = null!;

        [Inject]
        internal void Construct(
            IConnectedPlayer player,
            SnowballPacketHandler packetHandler)
        {
            _player = player;
            _packetHandler = packetHandler;
        }

        public void Start()
        {
            transform.localScale = Vector3.one * 0.25f;
            _packetHandler.SnowballPacketReceived += HandleSnowballPacket;
        }

        private void HandleSnowballPacket(SnowballPacket packet, IConnectedPlayer sendingPlayer)
        {
            if (_player.userId != sendingPlayer.userId)
                return;

            transform.SetLocalPositionAndRotation(packet.position, packet.rotation);
        }
    }
}
