using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;

namespace UnturnedRedistExample.NonPublicized
{
    /// <summary>
    /// Uses the plain (non-publicized) redist. Only PUBLIC Unturned members are
    /// accessible. Compare with <c>../Publicized</c>, which can also read
    /// non-public members. No AllowUnsafeBlocks needed here.
    /// </summary>
    public class NonPublicizedExamplePlugin : RocketPlugin
    {
        protected override void Load()
        {
            // Public Unturned API works fine with the plain redist:
            Logger.Log(
                $"[UnturnedRedistExample.NonPublicized] Loaded. " +
                $"Provider.maxPlayers = {Provider.maxPlayers} (public API).");

            // The line below would NOT compile here — the plain redist doesn't
            // expose non-public members, so the compiler can't see this field at
            // all. To read it, reference the .Publicized redist (see ../Publicized):
            //
            //     var ugc = Provider.isDedicatedUGCInstalled;
            //     // CS0117: 'Provider' does not contain a definition for 'isDedicatedUGCInstalled'
        }

        protected override void Unload()
        {
            Logger.Log("[UnturnedRedistExample.NonPublicized] Unloaded.");
        }
    }
}
