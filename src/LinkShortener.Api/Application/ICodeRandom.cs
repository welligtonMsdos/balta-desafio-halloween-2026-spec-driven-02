namespace LinkShortener.Api.Application;

public interface ICodeRandom
{
    int Next(int exclusiveMax);
}
