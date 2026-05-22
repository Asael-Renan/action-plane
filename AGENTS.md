# AGENTS.md - 5W2H Management

Default: **caveman full**. Use linguagem normal em codigo, commits, PRs e erros.

## Projeto

- App WPF desktop para acoes 5W2H.
- Stack: `.NET 8`, WPF, CommunityToolkit.Mvvm, SQLite/Dapper, OxyPlot, DI, Velopack.
- App project: `src/5W2H.App.csproj`.
- Test project: `tests/5W2H.Tests.csproj`.
- Namespace: `FiveW2H.App`.
- Fonte de verdade: codigo, `.csproj`, `.sln`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`.

## Ferramentas

- Serena MCP: usar sempre que possivel para contexto local, simbolos, referencias e memorias.
- Context7: usar para docs atuais de library, framework, SDK, API, CLI ou cloud.
- RTK: usar se disponivel; se ausente, declarar ausencia.
- Context7: `npx ctx7@latest library <nome> "<pergunta>"`, escolher ID, depois `npx ctx7@latest docs <id> "<pergunta>"`.
- Serena: `check_onboarding_performed`, ler memorias relevantes, preferir simbolos/referencias antes de arquivo inteiro, atualizar memoria so para fato duravel.

## Estrutura

```text
src/
|- 5W2H.App.csproj
|- App.xaml
|- Application/
|- Core/
|- Data/
|- Infrastructure/
|- Resources/
`- UI/

tests/
|- 5W2H.Tests.csproj
|- BackupServiceTests.cs
`- TaskServiceTests.cs
```

## Regras

- `Core`: modelos, enums e regras puras.
- `Application`: casos de uso, DTOs e contratos de aplicacao.
- `Data`: SQLite, Dapper, repositorios.
- `Infrastructure`: import/export, settings, updates e integracoes externas.
- `UI/ViewModels`: estado MVVM e comandos.
- `UI/Views`: XAML e code-behind minimo.
- `UI/Services`: dialogos WPF e interacao de tela.
- `tests`: xUnit deterministico.
- C# nullable habilitado; async com sufixo `Async`.
- DI por construtor + `ArgumentNullException`.
- Campos privados `_camelCase`; strings default `string.Empty`.
- ViewModels `partial` + `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`.
- Codigo em ingles; UI em portugues.
- Nao introduzir sem motivo: EF Core, nova UI framework, nova lib de graficos, SQL em ViewModel, WPF em `Core`.

## Mudancas e Validacao

- Checar `git status` antes de editar.
- Nao reverter mudancas do usuario.
- Escopo pequeno; sem refactor lateral.
- Mudou contrato publico: atualizar call sites.
- Mudou estrutura duravel: atualizar `AGENTS.md` e memorias Serena.
- Codigo: `dotnet build` + `dotnet test`.
- UI/XAML: `dotnet build`; `dotnet run --project src/5W2H.App.csproj` se viavel.
- Release: `dotnet publish -c Release src/5W2H.App.csproj -o publish`.

## Handoff

Responder sempre ao finalizar:

```markdown
## Handoff

| Campo | Valor |
|-------|-------|
| status | ok/parcial/bloqueado |
| pedido | ... |
| feito | ... |
| arquivos | `path` |
| contrato | sem_mudanca/mudou |
| serena | ok/indisponivel |
| caveman | full/lite/ultra/off |

### Validacao

| Comando | Resultado |
|---------|-----------|
| `dotnet build` | pass/fail/skip |
| `dotnet test` | pass/fail/skip |
| `dotnet run` | pass/fail/skip |
| publish | pass/fail/skip |

### Riscos / pendencias

- nenhum
```
