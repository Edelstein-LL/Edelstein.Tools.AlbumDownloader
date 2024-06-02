# Edelstein.Tools.AlbumDownloader

Edelstein.Tools.AlbumDownloader is a command-line tool to manipulate album assets of Love Live SIF2.

It can:

- Download (both Global and JP) album assets
- Extract assets from encrypted archives
- Mass convert all `.astc` files to `.png` using [astcenc](https://github.com/ARM-software/astc-encoder)

## Install

This program requires [.NET 8.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run and optionally [astcenc](https://github.com/ARM-software/astc-encoder) to convert `.astc` to `.png`.

Download respective [latest release](https://github.com/Arasfon/Edelstein.Tools.AlbumDownloader/releases/latest) executable for your OS and architecture.

If you need `.astc` conversion, download [astcenc](https://github.com/ARM-software/astc-encoder) for your OS and architecture and extract files from bin folder to the folder where you downloaded AlbumDowloader.

## Usage

This is a command-line tool, so use your terminal.

Every command have respective `--help`/`-h`/`-?` option to display help about command and it's options.

Download command requires from you AlbumUnitMMst.json and AlbumSeriesMMst.json from game's masterdata, formatted in camelCase.

### Commands

- `download` (`d`) — Downloads album
Options:
  - `-s, --scheme <Global|Jp>`       Download scheme used by the tool (Global or Jp) [default: `Jp`]
  - `-m, --mst-dir <mst-dir>`        Folder with AlbumUnitMMst.json and AlbumSeriesMMst.json [default: `.`]
  - `-o, --output-dir <output-dir>`  Target directory for downloaded files [default: `album`]
  - `--album-host <album-host>`      Host of album storage (if you need to override it) []
  - `--http`                         Use plain HTTP instead of HTTPS [default: `False`]
- `extract` (`x`) — Extracts all album archives
Options:
  - `-i, --input-dir <input-dir>`    Folder with original files [default: `album`]
  - `-o, --output-dir <output-dir>`  Target folder for extracted files [default: `album-extracted`]
- `convert` (`c`) — Converts all `.astc` files to `.png`
Options:
  - `-i, --input-dir <input-dir>`    Folder with extracted files [default: `album-extracted`]
  - `-o, --output-dir <output-dir>`  Target folder for converted files [default: `album-converted`]

## License

See [LICENSE](LICENSE)

## Used libraries

- [BookBeat.Akamai.EdgeAuthToken](https://github.com/BookBeat/EdgeAuth-Token-CSharp)
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
