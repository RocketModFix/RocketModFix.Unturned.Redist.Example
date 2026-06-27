using System.Reflection;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;

namespace UnturnedRedistExample.NonPublicized
{
    /// <summary>
    /// Uses the plain (non-publicized) redist. Public Unturned members are used
    /// directly; a NON-public member can only be reached via reflection — which
    /// is the cost of not publicizing. Compare with <c>../Publicized</c>, which
    /// reads the same field as a plain, compile-checked field access.
    /// </summary>
    public class NonPublicizedExamplePlugin : RocketPlugin
    {
        protected override void Load()
        {
            // Public members would be trivial here (e.g. Provider.maxPlayers).
            // But isDedicatedUGCInstalled is NON-public: a direct
            // `Provider.isDedicatedUGCInstalled` does not even compile against the
            // plain redist (CS0117). Without the .Publicized package, reflection
            // is the only way in:
            FieldInfo field = typeof(Provider).GetField(
                "isDedicatedUGCInstalled",
                BindingFlags.NonPublic | BindingFlags.Static);
            bool ugcInstalled = (bool)field.GetValue(null);

            Logger.Log(
                $"[UnturnedRedistExample.NonPublicized] Loaded. Read non-public " +
                $"isDedicatedUGCInstalled = {ugcInstalled} via REFLECTION — the name " +
                $"is a string (a rename breaks at runtime, not build) and every read " +
                $"pays a reflection + boxing cost.");
        }

        protected override void Unload()
        {
            Logger.Log("[UnturnedRedistExample.NonPublicized] Unloaded.");
        }
    }
}
