using Edelstein.Data.Msts;
using Edelstein.Tools.AlbumDownloader;

using ICSharpCode.SharpZipLib.Zip;

using Spectre.Console;

using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

Option<DownloadScheme> downloadSchemeOption =
    new(["--scheme", "-s"], () => DownloadScheme.Jp, "Download scheme used by the tool (Global or Jp)");
Option<string> downloadMstDirOption = new(["--mst-dir", "-m"], () => ".", "Folder with AlbumUnitMMst.json and AlbumSeriesMMst.json");
Option<string> downloadOutputOption = new(["--output-dir", "-o"], () => "album", "Target directory for downloaded files");
Option<string?> albumHostOption = new("--album-host", () => null, "Host of album storage");
Option<bool> httpOption = new("--http", () => false, "Use plain HTTP instead of HTTPS");

Command downloadCommand = new("download", "Downloads album")
{
    downloadSchemeOption,
    downloadMstDirOption,
    downloadOutputOption,
    albumHostOption,
    httpOption
};
downloadCommand.AddAlias("d");
downloadCommand.SetHandler(DownloadAlbum, downloadSchemeOption, downloadMstDirOption, downloadOutputOption, albumHostOption,
    httpOption);

Option<string> extractionInputOption = new(["--input-dir", "-i"], () => "album", "Folder with original files");
Option<string> extractionOutputOption = new(["--output-dir", "-o"], () => "album-extracted", "Target folder for extracted files");

Command extractCommand = new("extract", "Extracts all album archives")
{
    extractionInputOption,
    extractionOutputOption
};
extractCommand.AddAlias("x");
extractCommand.SetHandler(ExtractAlbum, extractionInputOption, extractionOutputOption);

Option<string> conversionInputOption = new(["--input-dir", "-i"], () => "album-extracted", "Folder with extracted files");
Option<string> conversionOutputOption = new(["--output-dir", "-o"], () => "album-converted", "Target folder for converted files");

Command convertCommand = new("convert", "Converts all .astc files to .png")
{
    conversionInputOption,
    conversionOutputOption
};
convertCommand.AddAlias("c");
convertCommand.SetHandler(ConvertAlbum, conversionInputOption, conversionOutputOption);

RootCommand rootCommand = [downloadCommand, extractCommand, convertCommand];

return await rootCommand.InvokeAsync(args);

async Task DownloadAlbum(DownloadScheme downloadScheme, string mstDir, string downloadPath, string? albumHost, bool http)
{
    const string defaultJpHost = "lovelive-schoolidolfestival2-album.akamaized.net";
    const string defaultGlHost = "album-sif2.lovelive-sif2.com";

    string baseUri = http ? "http://" : "https://";
    ITokenizedUriGenerator tokenizedUriGenerator;

    switch (downloadScheme)
    {
        case DownloadScheme.Jp:
        {
            if (albumHost is null)
                baseUri += defaultJpHost;
            else
                baseUri += albumHost;

            tokenizedUriGenerator = new AkamaiTokenizedUriGenerator();
            break;
        }
        case DownloadScheme.Global:
        {
            if (albumHost is null)
                baseUri += defaultGlHost;
            else
                baseUri += albumHost;

            tokenizedUriGenerator = new TencentTokenizedUriGenerator();
            break;
        }
        default:
            throw new ArgumentOutOfRangeException(nameof(downloadScheme), downloadScheme, null);
    }

    Directory.CreateDirectory(downloadPath);

    JsonSerializerOptions defaultJsonSerializationOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    List<AlbumSeriesMMst> seriesMsts;
    List<AlbumUnitMMst> unitMsts;

    List<string> allFilesUris = [];

    Console.WriteLine("Reading msts...");

    using (StreamReader sr = new(Path.Combine(mstDir, "AlbumSeriesMMst.json")))
    {
        seriesMsts = JsonSerializer.Deserialize<List<AlbumSeriesMMst>>(sr.ReadToEnd(), defaultJsonSerializationOptions)!;
    }

    using (StreamReader sr = new(Path.Combine(mstDir, "AlbumUnitMMst.json")))
    {
        unitMsts = JsonSerializer.Deserialize<List<AlbumUnitMMst>>(sr.ReadToEnd(), defaultJsonSerializationOptions)!;
    }

    Console.WriteLine("Constructing URIs...");

    allFilesUris.AddRange(seriesMsts
        .Select(x => x.ThumbnailPath)
        .Distinct()
        .Select(x => $"{baseUri}/GroupThumbnail/{x}.astc.zip"));

    allFilesUris.AddRange(unitMsts
        .Select(x => x.AlbumSeriesId)
        .Distinct()
        .Where(x => x != 0)
        .Select(x => $"{baseUri}/CardThumbnail/{x}.zip"));

    allFilesUris.AddRange(unitMsts
        .SelectMany(x => new[] { x.NormalCardId, x.RankMaxCardId })
        .Distinct()
        .Select(x => $"{baseUri}/Card/{x}.zip"));

    Console.WriteLine("Starting download...");

    HttpClient httpClient = new();

    await AnsiConsole.Progress()
        .HideCompleted(true)
        .StartAsync(async context =>
        {
            SemaphoreSlim semaphoreSlim = new(10);
            bool isPausedGlobally = false;

            ProgressTask globalProgressTask = context.AddTask("Global progress", true, allFilesUris.Count);

            await Task.WhenAll(allFilesUris.Select(DownloadAsset));

            async Task DownloadAsset(string stringUri)
            {
                Uri uri = new(stringUri);

                await semaphoreSlim.WaitAsync();

                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (isPausedGlobally)
                    await Task.Delay(100);

                ProgressTask progressTask = context.AddTask(uri.AbsolutePath);

                try
                {
                    using HttpResponseMessage response =
                        await httpClient.GetAsync(tokenizedUriGenerator.GenerateTokenizedUri(uri));
                    if (!response.IsSuccessStatusCode)
                        throw new Exception(response.StatusCode.ToString());

                    await using Stream httpStream = await response.Content.ReadAsStreamAsync();

                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(downloadPath, uri.AbsolutePath[1..]))!);

                    await using StreamWriter fileWriter = new(Path.Combine(downloadPath, uri.AbsolutePath[1..]), false);

                    await httpStream.CopyToWithProgressAsync(fileWriter.BaseStream, response.Content.Headers.ContentLength,
                        progressTask);
                }
                catch (Exception ex)
                {
                    isPausedGlobally = true;

                    AnsiConsole.WriteException(ex);

                    while (!AnsiConsole.Confirm("Continue?")) { }

                    isPausedGlobally = false;
                }

                progressTask.StopTask();
                globalProgressTask.Increment(1);
                semaphoreSlim.Release();
            }
        });

    AnsiConsole.WriteLine("Download completed!");
    AnsiConsole.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

async Task ExtractAlbum(string inputDir, string outputDir)
{
    Directory.CreateDirectory(outputDir);

    await AnsiConsole.Live(new Text("Initializing..."))
        .StartAsync(async liveDisplayContext =>
        {
            long totalFileCount = Directory.EnumerateFiles(inputDir, "*.zip", SearchOption.AllDirectories).Count() + 1;
            long currentFileNumber = 1;

            foreach (string filePath in Directory.EnumerateFiles(inputDir, "*.zip", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(inputDir, filePath);
                string fileOutputDir = Path.Combine(outputDir, Path.GetDirectoryName(relativePath)!,
                    Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath)));
                Directory.CreateDirectory(fileOutputDir);

                await using FileStream zipFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                await using ZipInputStream zipInputStream = new(zipFileStream);

                zipInputStream.Password = GenerateAlbumArchiveKey(filePath);

                Tree liveStatusTree = new(new TextPath(relativePath));
                Rows rows = new([Markup.FromInterpolated($"Processing [green]({currentFileNumber}/{totalFileCount})[/]:"), liveStatusTree]);
                liveDisplayContext.UpdateTarget(rows);

                while (zipInputStream.GetNextEntry() is { } entry)
                {
                    if (!entry.IsFile)
                        continue;

                    liveStatusTree.AddNode(new Markup(entry.Name));
                    liveDisplayContext.Refresh();

                    string entryOutputFileName = Path.Combine(fileOutputDir, entry.Name[2..] ?? "");

                    Directory.CreateDirectory(Path.GetDirectoryName(entryOutputFileName)!);

                    await using FileStream outputStream = File.Open(entryOutputFileName, FileMode.Create, FileAccess.Write);
                    await zipInputStream.CopyToAsync(outputStream);

                    liveStatusTree.Nodes.RemoveAt(liveStatusTree.Nodes.Count - 1);
                    liveStatusTree.AddNode(Markup.FromInterpolated($"[gray strikethrough]{entry.Name}[/]"));
                    liveDisplayContext.Refresh();
                }

                currentFileNumber++;
            }

            liveDisplayContext.UpdateTarget(Markup.FromInterpolated($"Processed [green]({currentFileNumber}/{totalFileCount})[/]"));
        });

    AnsiConsole.WriteLine("Success!");
    AnsiConsole.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

string GenerateAlbumArchiveKey(string filePath)
{
    string fileName = Path.GetFileName(filePath);

    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"sif2_{fileName}_album"))).ToLowerInvariant();
}

async Task ConvertAlbum(string inputDir, string outputDir)
{
    string? astcencPath = SearchAstcenc();
    if (astcencPath is null)
        return;

    Directory.CreateDirectory(outputDir);

    await AnsiConsole.Live(new Text("Initializing..."))
        .StartAsync(async liveDisplayContext =>
        {
            long totalFileCount = Directory.EnumerateFiles(inputDir, "*.astc", SearchOption.AllDirectories).Count() + 1;
            long currentFileNumber = 1;

            foreach (string filePath in Directory.EnumerateFiles(inputDir, "*.astc", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(inputDir, filePath);
                string fileOutputDir = Path.Combine(outputDir, Path.GetDirectoryName(relativePath)!);
                Directory.CreateDirectory(fileOutputDir);

                Rows rows = new([
                    Markup.FromInterpolated($"Processing [green]({currentFileNumber}/{totalFileCount})[/]:"), new TextPath(relativePath)
                ]);
                liveDisplayContext.UpdateTarget(rows);

                ProcessStartInfo processStartInfo = new()
                {
                    Arguments = $"-ds \"{filePath}\" \"{Path.Combine(fileOutputDir, Path.GetFileNameWithoutExtension(filePath))}.png\"",
                    FileName = astcencPath,
                    RedirectStandardOutput = true
                };

                Process process = Process.Start(processStartInfo)!;

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    AnsiConsole.Markup("[red]Something went wrong...[/]");
                    return;
                }

                currentFileNumber++;
            }

            liveDisplayContext.UpdateTarget(Markup.FromInterpolated($"Processed [green]({currentFileNumber}/{totalFileCount})[/]"));
        });

    AnsiConsole.WriteLine("Success!");
    AnsiConsole.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

string? SearchAstcenc()
{
    switch (RuntimeInformation.ProcessArchitecture)
    {
        case Architecture.X86:
        case Architecture.X64:
        {
            if (Avx2.IsSupported)
                return SearchByFilename("astcenc-avx2");
            if (Sse41.IsSupported)
                return SearchByFilename("astcenc-sse4.1");
            if (Sse2.IsSupported)
                return SearchByFilename("astcenc-sse2");

            throw new PlatformNotSupportedException();
        }
        case Architecture.Arm:
        case Architecture.Arm64:
        {
            return SearchByFilename("astcenc-neon");
        }
        default:
            throw new PlatformNotSupportedException();
    }

    string? SearchByFilename(string fileName)
    {
        if (OperatingSystem.IsWindows())
            fileName += ".exe";

        string nearPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (File.Exists(nearPath))
            return nearPath;

        string binPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", fileName);
        if (File.Exists(binPath))
            return binPath;

        AnsiConsole.MarkupLine("astcenc has not been found! " +
            "Have you downloaded it and extracted to program's location? " +
            "Download astcenc: [link]https://github.com/ARM-software/astc-encoder/releases/latest[/]");
        return null;
    }
}
