using Snowball.Managers;
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
            Container.BindInterfacesAndSelfTo<SnowballManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<SnowballSpawner>()
                .FromNewComponentOn(GameObject.CreatePrimitive(PrimitiveType.Sphere))
                .AsSingle().NonLazy();
        }
    }
}
