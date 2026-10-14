using LinkShortener.Api.Application;

namespace LinkShortener.Tests;

public sealed class CodeGeneratorTests
{
    [Fact]
    public void GeneratesSevenCharactersFromConfiguredAlphabet()
    {
        for (var index = 0; index < CodeGenerator.Alphabet.Length; index++)
        {
            var generated = new CodeGenerator(new FixedRandom(index)).Generate();
            Assert.Equal(7, generated.Length);
            Assert.Equal(new string(CodeGenerator.Alphabet[index], 7), generated);
            Assert.Matches("^[A-Z1-9]{7}$", generated);
            Assert.DoesNotContain('0', generated);
        }
    }

    private sealed class FixedRandom(int value) : ICodeRandom
    {
        public int Next(int exclusiveMax) => value;
    }
}
