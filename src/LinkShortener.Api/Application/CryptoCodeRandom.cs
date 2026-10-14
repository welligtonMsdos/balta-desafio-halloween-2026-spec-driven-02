using System.Security.Cryptography;

namespace LinkShortener.Api.Application;

public sealed class CryptoCodeRandom : ICodeRandom
{
    public int Next(int exclusiveMax) => RandomNumberGenerator.GetInt32(exclusiveMax);
}
