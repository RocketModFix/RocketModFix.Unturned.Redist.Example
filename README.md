# RocketModFix.Unturned.Redist.Example

Two minimal [RocketMod](https://github.com/RocketModFix/RocketModFix) plugins that read the **same non-public Unturned field two ways** — via reflection (the plain redist) and via the [`RocketModFix.Unturned.Redist`](https://github.com/RocketModFix/RocketModFix.Unturned.Redist) **`.Publicized`** package (a plain field access) — so you can see *why* publicizing is worth it.

> These are example **consumer** plugins — their namespaces (`UnturnedRedistExample.*`) are what *you'd* write in your own plugin; they just **reference** the RocketModFix packages.

## The two plugins

Both reference the **same game build** (`3.26.3.3`) and both read `SDG.Unturned.Provider.isDedicatedUGCInstalled` — a field that is **non-public** in the stock `Assembly-CSharp.dll`. The only difference is *how*:

| Project | Redist package | How it reads the non-public field |
| --- | --- | --- |
| [`NonPublicized/`](NonPublicized) | `RocketModFix.Unturned.Redist.Server` | **Reflection** — string-keyed, not compile-checked, slower |
| [`Publicized/`](Publicized) | `RocketModFix.Unturned.Redist.Server.Publicized` (+ `AllowUnsafeBlocks`) | **Direct field access** — compile-checked, fast |

## Why publicize instead of reflection?

A direct `Provider.isDedicatedUGCInstalled` **doesn't compile** against the plain redist — the package simply doesn't expose non-public members:

```
CS0117: 'Provider' does not contain a definition for 'isDedicatedUGCInstalled'
```

So without publicizing, your only option is **reflection** ([`NonPublicized/`](NonPublicized/NonPublicizedExamplePlugin.cs)):

```csharp
FieldInfo field = typeof(Provider).GetField(
    "isDedicatedUGCInstalled", BindingFlags.NonPublic | BindingFlags.Static);
bool ugcInstalled = (bool)field.GetValue(null);
```

Downsides:
- **Not compile-checked.** The member name is a string — a rename or typo compiles fine and throws at **runtime** (`NullReferenceException` on the missing `field`), on your live server.
- **Harder to read.** No IntelliSense, casts everywhere, three lines for one read.
- **Slower.** Every access pays a reflection lookup + boxing; even caching the `FieldInfo` stays indirect.

The **`.Publicized`** package ([`Publicized/`](Publicized/PublicizedExamplePlugin.cs)) rewrites that member to public, so the same read is one normal line:

```csharp
bool ugcInstalled = Provider.isDedicatedUGCInstalled;
```

- **Compile-checked** — a rename breaks the *build*, not your server.
- **Reads like ordinary code** — full IntelliSense, no casts.
- **Plain field-access speed.**

The one requirement: set `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the `.csproj`, which lets Unturned's Mono runtime skip the access check on the originally-private member.

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
3. Start the server and watch the console — both read the same value, the hard way and the easy way:

   ```
   [UnturnedRedistExample.NonPublicized] Loaded. Read non-public isDedicatedUGCInstalled = False via REFLECTION ...
   [UnturnedRedistExample.Publicized]    Loaded. Read NON-public Provider.isDedicatedUGCInstalled = False via the .Publicized redist.
   ```

Seeing those lines confirms each plugin **loaded** and read the non-public member **at runtime**. (For the publicized one, that also proves `AllowUnsafeBlocks` did its job — if publicization were broken you'd get a `FieldAccessException` instead.)

## License

MIT — see [LICENSE](LICENSE).
