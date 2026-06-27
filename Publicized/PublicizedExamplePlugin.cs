using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;

namespace UnturnedRedistExample.Publicized
{
    /// <summary>
    /// Uses the <c>.Publicized</c> redist, so it can read members that are
    /// non-public in the stock game. Compare with <c>../NonPublicized</c>, which
    /// references the plain redist and can only touch public API.
    /// </summary>
    public class PublicizedExamplePlugin : RocketPlugin
    {
        protected override void Load()
        {
            // isDedicatedUGCInstalled is NON-public in stock Unturned. This line
            // compiles only because of the .Publicized redist, and runs only
            // because the .csproj sets AllowUnsafeBlocks.
            var ugcInstalled = Provider.isDedicatedUGCInstalled;

            Logger.Log(
                $"[UnturnedRedistExample.Publicized] Loaded. Read NON-public " +
                $"Provider.isDedicatedUGCInstalled = {ugcInstalled} via the .Publicized redist.");
        }

        protected override void Unload()
        {
            Logger.Log("[UnturnedRedistExample.Publicized] Unloaded.");
        }
    }
}
