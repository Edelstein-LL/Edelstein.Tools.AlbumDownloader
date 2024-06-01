using System.Security.Cryptography;
using System.Text;

namespace Edelstein.Tools.AlbumDownloader;

public class TencentTokenizedUriGenerator : ITokenizedUriGenerator
{
    private const string Key = "YCuWEFAq7s6g9728i15ON";

    public Uri GenerateTokenizedUri(Uri uri)
    {
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new Uri(uri,
            $"?sign={Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes($"{Key}{uri.AbsolutePath}{currentTimestamp}"))).ToLower()}&t={currentTimestamp}");
    }
}
