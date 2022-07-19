using IPA.Utilities;
using MultiplayerCore.Networking;
using Snowball.Networking;
using Snowball.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Snowball.Managers
{
    internal class SnowballManager : IInitializable, IDisposable
    {
        private readonly FieldAccessor<MultiplayerLobbyAvatarManager, Dictionary<string, MultiplayerLobbyAvatarController>>.Accessor _playerAvatarMapAccessor
            = FieldAccessor<MultiplayerLobbyAvatarManager,Dictionary<string, MultiplayerLobbyAvatarController>>.GetAccessor("_playerIdToAvatarMap");

        public event Action<SnowballGrabPacket, IConnectedPlayer> SnowballGrabReceived = null!;
        public event Action<SnowballReleasePacket, IConnectedPlayer> SnowballReleaseReceived = null!;

        public HashSet<Guid> snowballs = new();

        private readonly MpPacketSerializer _packetSerializer;
        private readonly DiContainer _container;
        private readonly Dictionary<string, MultiplayerLobbyAvatarController> _playerAvatarMap;

        [Inject]
        internal SnowballManager(
            MpPacketSerializer packetSerializer,
            DiContainer container,
            MultiplayerLobbyAvatarManager lobbyAvatarManager)
        {
            _packetSerializer = packetSerializer;
            _container = container;
            _playerAvatarMap = _playerAvatarMapAccessor(ref lobbyAvatarManager);
        }

        public void Initialize()
        {
            _packetSerializer.RegisterCallback<SnowballGrabPacket>(HandleGrabPacket);
            _packetSerializer.RegisterCallback<SnowballReleasePacket>(HandleReleasePacket);
        }

        public void Dispose()
        {
            _packetSerializer.UnregisterCallback<SnowballGrabPacket>();
            _packetSerializer.UnregisterCallback<SnowballReleasePacket>();
        }

        public void HandleGrabPacket(SnowballGrabPacket packet, IConnectedPlayer player)
        {
            if (!snowballs.Contains(packet.id))
            {
                var avatar = GetPlayerAvatar(player);
                var snowball = CreateSnowball(packet.id);
                snowball.transform.SetParent(avatar.transform);
            }
            SnowballGrabReceived?.Invoke(packet, player);
        }

        public void HandleReleasePacket(SnowballReleasePacket packet, IConnectedPlayer player)
        {
            SnowballReleaseReceived?.Invoke(packet, player);
        }

        public SnowballObject CreateSnowball(Guid id)
        {
            var snowball = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<SnowballObject>();
            snowball.id = id;
            snowballs.Add(id);
            _container.Inject(snowball);
            return snowball;
        }

        public void RemoveSnowball(Guid id)
        {
            snowballs.Remove(id);
        }

        public MultiplayerLobbyAvatarController GetPlayerAvatar(IConnectedPlayer player)
            => _playerAvatarMap[player.userId];
    }
}
