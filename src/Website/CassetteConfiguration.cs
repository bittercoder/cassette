using Cassette;
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add<StylesheetBundle>("assets/styles");
            bundles.Add<StylesheetBundle>("assets/iestyles", b => b.Condition = "IE");

            bundles.AddPerSubDirectory<ScriptBundle>("assets/scripts");
            bundles.AddUrlWithLocalAssets(
                "//ajax.googleapis.com/ajax/libs/jquery/1.6.3/jquery.min.js",
                new LocalAssetSettings
                {
                    FallbackCondition = "!window.jQuery",
                    Path =  "assets/scripts/jquery"
                }
            );

            var pluginScripts = new ScriptBundle("plugin/scripts");
            pluginScripts.Processor = new ScriptPipeline();
            pluginScripts.Assets.Add(new ResourceAsset("assets/scripts/plugin/script1.js", GetType().Assembly, pluginScripts));
            pluginScripts.Assets.Add(new ResourceAsset("assets/scripts/plugin/script2.js", GetType().Assembly, pluginScripts));
            pluginScripts.Assets.Add(new ResourceAsset("assets/scripts/plugin/script3.js", GetType().Assembly, pluginScripts));
            
            bundles.Add(pluginScripts);
        }
    }
}