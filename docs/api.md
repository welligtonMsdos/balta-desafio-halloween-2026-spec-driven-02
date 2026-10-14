# API de links

Requisitos: SDK .NET 10. Execute `dotnet run --project src/LinkShortener.Api` na
raiz do repositório. Por padrão, a aplicação usa `links.db` no diretório de
execução. A string de conexão pode ser alterada em `ConnectionStrings:Links`.
As migrações são aplicadas ao iniciar.

- `POST /api/links`: JSON com `targetUrl`, `code` opcional e `expiresAtUtc`
  opcional. Retorna `201` com `shortUrl` e `code`.
- `GET /{code}`: retorna `302` para o destino, `404` se não existe e `410` se
  expirou.
- `GET /api/links/{code}/stats`: retorna `code` e `accessCount`, sem chave.

`expiresAtUtc` aceita data e hora com fuso explícito. A data é convertida para
UTC e o link vale até `23:59:59.9999999` dessa data. Erros de validação
retornam `400` com `ProblemDetails` em português. Para executar os testes:
`dotnet test LinkShortener.slnx`.
