namespace LinkShortener.Api.Application;

public enum ResolveLinkStatus { Found, Missing, Expired }

public sealed record ResolveLinkResult(ResolveLinkStatus Status, string? TargetUrl = null);
