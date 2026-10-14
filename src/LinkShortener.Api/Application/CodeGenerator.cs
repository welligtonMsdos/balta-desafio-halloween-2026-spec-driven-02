namespace LinkShortener.Api.Application;

public sealed class CodeGenerator(ICodeRandom random)
{
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string Generate() => new(Enumerable.Range(0, 7)
        .Select(_ => Alphabet[random.Next(Alphabet.Length)]).ToArray());
}
