# Tarefas

## Alinhamento necessário no plano

- O contrato de `GET /api/links/{code}/stats`, D-05 e `ManagementKeyHash` exigem chave de gerenciamento, mas RF-07 e o critério de aceite permitem consulta pública por código. `DC-03` não existe na especificação. T-00 corrige o plano; T-09 implementa a consulta pública.
- D-03 pede um alfabeto sem caracteres ambíguos, mas não lista os caracteres escolhidos. T-00 registra esse conjunto dentro de `A-Z` e `1-9`, para que a geração e seus testes sejam verificáveis.
- A arquitetura do plano atribui apenas RN-01 a RN-04 ao `LinkService`, mas RN-05 e RN-06 também são regras de negócio. T-00 explicita essa responsabilidade.

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-00 | Corrigir o plano: consulta pública de estatísticas, remoção da chave de gerenciamento, alfabeto de geração explícito dentro de `A-Z` e `1-9` e responsabilidade do serviço por RN-01 a RN-06 | RF-02, RF-07, RN-05, RN-06, D-03, Constituição | | Plano e especificação deixam de divergir; o alfabeto de geração fica definido |
| T-01 | Criar projetos .NET 10 de Minimal API e testes xUnit, com estrutura de endpoints, aplicação e dados | Constituição, Plano | T-00 | Solução compila e os testes executam |
| T-02 | Modelar `Link`, repositório EF Core, configuração SQLite, migração e índice único para `Code` | RF-08, RN-01, D-01, D-04 | T-01 | Migração aplica, índice impede duplicatas e dados sobrevivem à reabertura do banco |
| T-03 | Gerar códigos automáticos de 7 caracteres com o alfabeto definido em D-03, limitado a `A-Z` e `1-9` | RF-02, RN-05, D-03 | T-00, T-01 | Testes unitários verificam comprimento, alfabeto definido e ausência de `0`, com fonte aleatória controlada |
| T-04 | Validar URL de destino: esquema HTTP/HTTPS, até 2048 caracteres e acesso público | RN-02, casos de borda | T-01 | Testes unitários cobrem esquemas inválidos, tamanho excedido e destinos não públicos |
| T-05 | Criar link automático pelo `LinkService` e `POST /api/links`, retornando `shortUrl` e `code` | RF-01, Plano | T-02, T-03, T-04 | Testes unitários cobrem as regras; teste de integração retorna 201 e contrato completo |
| T-06 | Aceitar código próprio de 4 a 20 caracteres, validando `A-Z` e `1-9` e recusando código já usado | RF-03, RN-01, RN-05, D-04 | T-05 | Testes unitários cobrem limites e caracteres proibidos; integração confirma 409 para duplicata, inclusive disputa concorrente |
| T-07 | Repetir a geração e a gravação quando um código automático colidir, inclusive por violação do índice único | RN-06, D-04, caso de borda | T-05 | Teste com colisão simulada comprova criação com outro código |
| T-08 | Redirecionar por `GET /{code}` com 302 e responder 404 para código inexistente | RF-04, Plano, caso de borda | T-05 | Testes de integração cobrem redirecionamento direto e 404 |
| T-09 | Expor `GET /api/links/{code}/stats` com total de acessos consultável por qualquer pessoa que conheça o código | RF-07, critério de aceite | T-05, T-00 | Teste de integração consulta o total sem autenticação ou chave |
| T-10 | Incrementar o contador por `UPDATE` atômico em cada acesso válido, inclusive repetido ou automatizado, sem reduzi-lo | RF-05, RN-04, D-02 | T-08 | Testes unitários cobrem a regra; integração concorrente confirma que nenhum incremento se perde |
| T-11 | Validar a expiração na criação e no acesso, em UTC, mantendo o link válido até o fim da data informada | RF-06, RN-03, casos de borda, critérios de aceite | T-05, T-08, T-10 | Testes unitários cobrem data passada e último instante válido; integração confirma 410 após expiração e contador inalterado |
| T-12 | Responder falhas de validação com HTTP 400 e `ProblemDetails`, com mensagens em português | Constituição, RF-03, RN-02, RN-05, casos de borda | T-04, T-06, T-11 | Testes de integração verificam status, formato e mensagens dos casos inválidos |
| T-13 | Verificar persistência após reinício da aplicação usando o mesmo arquivo SQLite | RF-08, critério de aceite | T-02, T-08, T-09, T-10 | Integração reinicia a aplicação e confirma redirecionamento e total de acessos persistidos |

Os endpoints devem apenas validar entrada, chamar o `LinkService` e traduzir seu resultado HTTP; o serviço concentra as regras de negócio e usa o repositório para persistência. Nomes de classes e arquivos ficam em inglês. Commits seguem `tipo(escopo): descrição` e incluem o identificador do requisito, conforme a constituição.
