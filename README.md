# Godot 4 Web .NET prototype

This is a demo project to showcase exporting C# projects to the web platform. It was built using the new web export support coming to Godot 4.

Inspired by [SharpLab](https://sharplab.io/), the demo uses Roslyn to compile arbitraty C# code at runtime and invokes it using the Mono interpreter.

> [!NOTE]
> The demo requires multi-threading support which uses an experimental Mono runtime.

See more information about web exports for C# projects in the [_Live from GodotCon Boston: Web .NET prototype_](https://godotengine.org/article/live-from-godotcon-boston-web-dotnet-prototype/) blog article and the Pull Request [GH-106125](https://github.com/godotengine/godot/pull/106125) that implements it.

## Try the demo on your browser

Try it out on https://lab.godotengine.org/godot-dotnet-web/

## Export the project

Requirements:

- Godot editor and template that supports exporting C# projects to web.
	- PR: [GH-106125](https://github.com/godotengine/godot/pull/106125)
- .NET SDK 9.0+ with `wasm-tools` workload installed.

> [!CAUTION]
> Currently, the output `index.js` file needs to be manually edited to remove the `DOTNET.setup` call.

Export the game from the Godot editor's export dialog or use this command on a console:

```bash
godot --headless --export-release "Web" "out/index.html"
```

> [!NOTE]
> The output path `out/index.html` puts the exported project under the `out` directory.

You can also use the provided [serve.py](./serve.py) python script to serve the exported project on the local machine with a basic web server. This script was taken from the [main Godot repository](https://github.com/godotengine/godot/blob/e37c6261ea48b1a339c469282df7e9b7c073a72f/platform/web/serve.py).

```bash
python serve.py --root out
```

> [!NOTE]
> The `--root` option specifies the directory that contains the exported project.

## Third party notices

This project includes resources such as the .NET logo that is owned by the Microsoft corporation with no affiliation to this demo.

See also [THIRD-PARTY-NOTICES](./THIRD-PARTY-NOTICES.txt).
