using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;

namespace UnturnedRedistExample
{
    /// <summary>
    /// Minimal RocketMod plugin that demonstrates consuming the
    /// <c>RocketModFix.Unturned.Redist.Server.Publicized</c> NuGet package.
    ///
    /// On load it reads <see cref="Provider.isDedicatedUGCInstalled"/> — a field
    /// that is non-public in the stock <c>Assembly-CSharp.dll</c>. That line only
    /// compiles because we reference the <c>.Publicized</c> redist (which rewrites
    /// the field to public), and only runs because the .csproj sets
    /// <c>AllowUnsafeBlocks</c> (which lets Mono skip the runtime access check).
    /// Seeing the log line in the server console is end-to-end proof that
    /// publicized access works.
    /// </summary>
    public class UnturnedRedistExamplePlugin : RocketPlugin
    {
        protected override void Load()
        {
            // isDedicatedUGCInstalled is non-public in stock Unturned — reachable
            // here only through the .Publicized redist.
            var ugcInstalled = Provider.isDedicatedUGCInstalled;

            Logger.Log(
                $"[UnturnedRedistExample] Loaded. Read non-public " +
                $"Provider.isDedicatedUGCInstalled = {ugcInstalled} via the .Publicized redist.");
        }

        protected override void Unload()
        {
            Logger.Log("[UnturnedRedistExample] Unloaded.");
        }
    }
}
