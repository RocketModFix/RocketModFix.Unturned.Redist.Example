# RocketModFix.Unturned.Redist.Example

Two minimal [RocketMod](https://github.com/RocketModFix/RocketModFix) plugins that show how to consume the RocketModFix NuGet packages in an Unturned server plugin — and, side by side, exactly what the [`RocketModFix.Unturned.Redist`](https://github.com/RocketModFix/RocketModFix.Unturned.Redist) **`.Publicized`** package changes.

> These are example **consumer** plugins — their namespaces (`UnturnedRedistExample.*`) are what *you'd* write in your own plugin; they just **reference** the RocketModFix packages.

## The two plugins

| Project | Redist package referenced | `AllowUnsafeBlocks` | Reads non-public members? |
| --- | --- | --- | --- |
| [`NonPublicized/`](NonPublicized) | `RocketModFix.Unturned.Redist.Server` | no | ❌ public API only |
| [`Publicized/`](Publicized) | `RocketModFix.Unturned.Redist.Server.Publicized` | **yes** | ✅ |

Both reference the **same game build** (`3.26.3.3`) and are otherwise identical RocketMod plugins. The redist package is the only meaningful difference — that's the whole lesson.

### `NonPublicized` — public API only

Reads a **public** member and logs it. The non-public member is shown commented out, because it does **not** compile against the plain redist:

```csharp
Logger.Log($"... Provider.maxPlayers = {Provider.maxPlayers} (public API).");

//   var ugc = Provider.isDedicatedUGCInstalled;
//   // CS0117: 'Provider' does not contain a definition for 'isDedicatedUGCInstalled'
```

The plain redist doesn't expose non-public members, so the compiler can't see the field at all.

### `Publicized` — non-public members too

Reads `SDG.Unturned.Provider.isDedicatedUGCInstalled` — **non-public** in the stock `Assembly-CSharp.dll`:

```csharp
var ugcInstalled = Provider.isDedicatedUGCInstalled; // compiles via the .Publicized package
Logger.Log($"... isDedicatedUGCInstalled = {ugcInstalled} ...");
```

This compiles only because the `.Publicized` package rewrites that member to public, and runs only because the `.csproj` sets `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` (which lets Unturned's Mono runtime skip the access check).

## Packages used (both plugins)

| Package | Why |
| --- | --- |
| [`RocketModFix.Rocket.Unturned`](https://www.nuget.org/packages/RocketModFix.Rocket.Unturned) | RocketMod API — the `RocketPlugin` base class and `Logger`. |
| [`RocketModFix.Unturned.Redist.Server`](https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Server) / [`.Publicized`](https://www.nuget.org/packages/RocketModFix.Unturned.Redist.Server.Publicized) | Unturned's server assemblies. `.Publicized` additionally exposes non-public members. |
| [`RocketModFix.UnityEngine.Redist`](https://www.nuget.org/packages/RocketModFix.UnityEngine.Redist) | UnityEngine assemblies — required because Unturned types derive from `UnityEngine.MonoBehaviour`. |

See the [redist README](https://github.com/RocketModFix/RocketModFix.Unturned.Redist#using-a-publicized-package) for more on `.Publicized` and `AllowUnsafeBlocks`.

## Build

```bash
dotnet build -c Release
```

Builds both projects (via `UnturnedRedistExample.sln`); the DLLs land in each project's `bin/Release/net461/`. CI builds both on every push — see [`.github/workflows/build.yaml`](.github/workflows/build.yaml).

## Verify it loads on a server

1. Install [RocketModFix](https://github.com/RocketModFix/RocketModFix#installation) on an Unturned dedicated server (copy the `Rocket.Unturned` folder into `Modules/`) and start it once so it generates `Servers/<ServerID>/Rocket/`.
2. Copy either built DLL into `Servers/<ServerID>/Rocket/Plugins/` (DLLs go directly in `Plugins/`, not a subfolder).
3. Start the server and watch the console:

   ```
   [UnturnedRedistExample.NonPublicized] Loaded. Provider.maxPlayers = 24 (public API).
   [UnturnedRedistExample.Publicized] Loaded. Read NON-public Provider.isDedicatedUGCInstalled = False via the .Publicized redist.
   ```

The `Publicized` line is the key one: it proves the non-public member was read **at runtime** (so `AllowUnsafeBlocks` did its job). If publicization were broken you'd get a `FieldAccessException` ("is inaccessible") instead.

## License

MIT — see [LICENSE](LICENSE).
