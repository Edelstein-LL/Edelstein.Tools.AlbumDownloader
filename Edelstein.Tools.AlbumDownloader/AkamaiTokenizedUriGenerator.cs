using BookBeat.Akamai.EdgeAuthToken;

namespace Edelstein.Tools.AlbumDownloader;

public class AkamaiTokenizedUriGenerator : ITokenizedUriGenerator
{
    private const string Key = "215b773e2ce4ed4402edd8bb5a6729fbf502941c16f4afa0f91f0df0e671e8fc";

    private readonly AkamaiTokenConfig _tokenConfig = new()
    {
        Window = 3600,
        Acl = "*",
        Key = Key
    };

    private readonly AkamaiTokenGenerator _tokenGenerator = new();

    public Uri GenerateTokenizedUri(Uri uri) =>
        new(uri, $"?__gda__={_tokenGenerator.GenerateToken(_tokenConfig)}");
}
