using System.Net;
using System.Net.Sockets;

namespace LinkShortener.Api.Application;

public sealed class TargetUrlValidator(IHostAddressResolver resolver)
{
    public async Task<string?> ValidateAsync(string? targetUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetUrl) || targetUrl.Length > 2048 ||
            !Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            return "A URL de destino deve usar HTTP ou HTTPS e ter até 2048 caracteres.";

        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
            return "A URL de destino deve ser publicamente acessível.";

        try
        {
            var addresses = IPAddress.TryParse(uri.Host, out var parsed)
                ? [parsed]
                : await resolver.ResolveAsync(uri.Host, cancellationToken);
            if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
                return "A URL de destino deve ser publicamente acessível.";
        }
        catch (SocketException)
        {
            return "A URL de destino deve ser publicamente acessível.";
        }

        return null;
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var a = bytes[0];
            var b = bytes[1];
            return a is >= 1 and <= 223 &&
                !(a == 10 || a == 127 || a == 169 && b == 254 ||
                  a == 172 && b is >= 16 and <= 31 || a == 192 && b == 168 ||
                  a == 100 && b is >= 64 and <= 127 || a == 192 && b == 0 && bytes[2] == 2 ||
                  a == 198 && (b == 18 || b == 19 || b == 51 && bytes[2] == 100) ||
                  a == 203 && b == 0 && bytes[2] == 113);
        }

        return address.AddressFamily == AddressFamily.InterNetworkV6 &&
            (bytes[0] & 0xE0) == 0x20 &&
            !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8);
    }
}
