# RocketModFix.Unturned.Redist.Example

A minimal [RocketMod](https://github.com/RocketModFix/RocketModFix) plugin showing how to consume the RocketModFix NuGet packages in an Unturned server plugin — in particular the [`RocketModFix.Unturned.Redist`](https://github.com/RocketModFix/RocketModFix.Unturned.Redist) **`.Publicized`** package, which lets a plugin read members that are non-public in the stock game.

> This is an example **consumer** plugin. Its own namespace is `UnturnedRedistExample` — i.e. what *you'd* write in your own plugin — it just **references** the RocketModFix packages.

## What it demonstrates

[`UnturnedRedistExamplePlugin.cs`](UnturnedRedistExamplePlugin.cs) is one class. On load it reads `SDG.Unturned.Provider.isDedicatedUGCInstalled` — a field that is **non-public** in the stock `Assembly-CSharp.dll` — and logs it:

```csharp
protected override void Load()
{
    // non-public in stock Unturned; reachable only via the .Publicized redist
    var ugcInstalled = Provider.isDedicatedUGCInstalled;
    Logger.Log($"[UnturnedRedistExample] Loaded. ... = {ugcInstalled} ...");
}
```

That single line is the whole point:

- It **compiles** only because the project references `RocketModFix.Unturned.Redist.Server.Publicized` (the publicizer rewrites that field to public).
- It **runs** only because the `.csproj` sets `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`, which lets Unturned's Mono runtime skip the access check.

## Packages used

| Package | Why |
| --- | --- |
| [`RocketModFix.Rocket.Unturned`](https://www.nuget.org/packages/RocketModFix.Rocket.Unturned) | The RocketMod API — provides the `RocketPlugin` base class and `Logger`. |
| [`RocketModFix.Unturned.Redist.Server.Publicized`](https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Server.Publicized) | Unturned's server assemblies, **publicized** so the plugin can touch non-public members. Use [`…Server`](https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Server) (without `.Publicized`) if you don't need that. |
| [`RocketModFix.UnityEngine.Redist`](https://www.nuget.org/packages/RocketModFix.UnityEngine.Redist) | UnityEngine assemblies — required because Unturned types derive from `UnityEngine.MonoBehaviour`. |

See the [redist README](https://github.com/RocketModFix/RocketModFix.Unturned.Redist#using-a-publicized-package) for more on `.Publicized` and `AllowUnsafeBlocks`.

## Build

```bash
dotnet build -c Release
```

The plugin DLL lands in `bin/Release/net461/UnturnedRedistExample.dll`. CI builds it on every push — see [`.github/workflows/build.yaml`](.github/workflows/build.yaml).

## Verify it loads on a server

1. Install [RocketModFix](https://github.com/RocketModFix/RocketModFix#installation) on an Unturned dedicated server (copy the `Rocket.Unturned` folder into `Modules/`) and start it once so it generates `Servers/<ServerID>/Rocket/`.
2. Copy the built `UnturnedRedistExample.dll` into `Servers/<ServerID>/Rocket/Plugins/` (the DLL goes directly in `Plugins/`, not a subfolder).
3. Start the server and watch the console for:

   ```
   [UnturnedRedistExample] Loaded. Read non-public Provider.isDedicatedUGCInstalled = ... via the .Publicized redist.
   ```

That line confirms two things at once: the plugin **loaded**, and the **publicized** member was read successfully **at runtime** (so `AllowUnsafeBlocks` did its job). If publicization were broken you'd instead get a `FieldAccessException` / "is inaccessible" error.

## License

MIT — see [LICENSE](LICENSE).
