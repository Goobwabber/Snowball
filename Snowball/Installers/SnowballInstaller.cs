using SiraUtil.Extras;
using SiraUtil.Objects.Multiplayer;
using Snowball.Networking;
using Snowball.Objects;
using UnityEngine;
using Zenject;

namespace Snowball.Installers
{
    public class SnowballInstaller : Installer
    {
        private readonly Config _config;

        public SnowballInstaller(
            Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<SnowballPacketHandler>().AsSingle();
            Container.Bind<LocalSnowball>().ToSelf()
                .FromNewComponentOn(GameObject.CreatePrimitive(PrimitiveType.Sphere))
                .AsSingle().NonLazy();

            Container.RegisterRedecorator(new LobbyAvatarRegistration(DecorateAvatar));
        }

        private MultiplayerLobbyAvatarController DecorateAvatar(MultiplayerLobbyAvatarController original)
        {
            var snowball = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<ConnectedSnowball>();
            snowball.transform.SetParent(original.transform);

            return original;
        }
    }
}
