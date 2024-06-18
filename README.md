# Edelstein.Tools.AlbumDownloader

Edelstein.Tools.AlbumDownloader is a command-line tool to manipulate album assets of Love Live SIF2.

It can:

- Download (both Global and JP) album assets
- Extract assets from encrypted archives
- Mass convert all `.astc` files to `.png` using [astcenc](https://github.com/ARM-software/astc-encoder)

## Install

This program requires the [.NET 8.0 runtime](https://dot.net/download) to run and optionally [astcenc](https://github.com/ARM-software/astc-encoder) to convert `.astc` to `.png`.

Download respective [latest release](https://github.com/Edelstein-LL/Edelstein.Tools.AlbumDownloader/releases/latest) executable for your OS and architecture.

If you need `.astc` conversion, download [astcenc](https://github.com/ARM-software/astc-encoder) for your OS and architecture and extract the files from the bin directory to the directory where you downloaded AlbumDownloader.

## Usage

```bash
./Edelstein.Tools.AlbumDownloader [command] [options]
```

Every command have respective `--help`/`-h`/`-?` option to display help about the command and its options.

> [!NOTE]
> The download command requires AlbumUnitMMst.json and AlbumSeriesMMst.json from the game's masterdata, formatted in camelCase.

### Commands

- `download` (`d`) — Downloads album
  - `-s, --scheme <Global|Jp>`                       Download scheme used by the tool (Global or Jp) [default: `Jp`]
  - `-m, --mst-dir <mst-dir>`                        Directory with AlbumUnitMMst.json and AlbumSeriesMMst.json [default: `.`]
  - `-o, --output-dir <output-dir>`                  Target directory for downloaded files [default: `album`]
  - `-p, --parallel-downloads <parallel-downloads>`  Count of parallel downloads [default: `10`]
  - `--album-host <album-host>`                      Host of album storage []
  - `--http`                                         Use plain HTTP instead of HTTPS [default: `False`]
- `extract` (`x`) — Extracts all album archives
  - `-i, --input-dir <input-dir>`    Directory with original files [default: `album`]
  - `-o, --output-dir <output-dir>`  Target directory for extracted files [default: `album-extracted`]
- `convert` (`c`) — Converts all `.astc` files to `.png`
  - `-i, --input-dir <input-dir>`    Directory with extracted files [default: `album-extracted`]
  - `-o, --output-dir <output-dir>`  Target directory for converted files [default: `album-converted`]

## License

See [LICENSE](LICENSE)

## Used libraries

- [BookBeat.Akamai.EdgeAuthToken](https://github.com/BookBeat/EdgeAuth-Token-CSharp)
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
