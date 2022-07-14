using IPA;
using IPALogger = IPA.Logging.Logger;
using Conf = IPA.Config.Config;
using HarmonyLib;
using IPA.Loader;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using Snowball.Installers;

namespace Snowball
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public const string ID = "com.goobwabber.snowball";

        internal static IPALogger Logger = null!;
        internal static Config Config = null!;

        private readonly Harmony _harmony;
        private readonly PluginMetadata _metadata;

        [Init]
        public Plugin(IPALogger logger, Conf conf, Zenjector zenjector, PluginMetadata pluginMetadata)
        {
            Config config = conf.Generated<Config>();
            _harmony = new Harmony(ID);
            _metadata = pluginMetadata;
            Logger = logger;
            Config = config;

            zenjector.UseMetadataBinder<Plugin>();
            zenjector.UseLogger(logger);
            zenjector.UseSiraSync(SiraUtil.Web.SiraSync.SiraSyncServiceType.GitHub, "Goobwabber", "Snowball");
            zenjector.Install<SnowballInstaller>(Location.Menu, config);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(_metadata.Assembly);
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
