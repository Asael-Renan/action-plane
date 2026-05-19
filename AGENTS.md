# AGENTS.md - 5W2H Management

Guia curto para agentes. Default: **caveman full**. Codigo, commits, PRs e erros: linguagem normal.

## Projeto

| Campo | Valor |
|-------|-------|
| App | WPF desktop para acoes 5W2H |
| Stack | `.NET 8` · WPF · CommunityToolkit.Mvvm · SQLite/Dapper · OxyPlot · DI · Velopack |
| App project | `src/5W2H.App.csproj` |
| Test project | `tests/5W2H.Tests.csproj` |
| Namespace | `FiveW2H.App` |

Fonte de verdade: codigo, `.csproj`, `.sln`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`.

## Ferramentas

| Ferramenta | Regra |
|------------|-------|
| Serena MCP | Usar sempre que possivel para contexto local, simbolos, referencias e memorias |
| Context7 | Usar para docs atuais de library, framework, SDK, API, CLI ou cloud |
| RTK | Usar se estiver disponivel no contexto; se nao estiver, declarar ausencia |

Context7:

1. `npx ctx7@latest library <nome-oficial> "<pergunta completa>"`
2. Escolher melhor ID `/org/project`
3. `npx ctx7@latest docs <libraryId> "<pergunta completa>"`

Se Context7 falhar por quota: citar `npx ctx7@latest login` ou `CONTEXT7_API_KEY`. Se falhar por rede/DNS no sandbox: rerodar fora do sandbox.

## Serena

Fluxo padrao:

1. `check_onboarding_performed`
2. Ler memorias relevantes
3. Usar simbolos antes de arquivo inteiro
4. Usar referencias antes de mudar contrato publico
5. Atualizar memoria so para fato duravel do projeto

Memorias esperadas: `project_overview`, `style_and_conventions`, `suggested_commands`, `task_completion_checklist`.

## Estrutura

```text
src/
|- 5W2H.App.csproj
|- App.xaml
|- Core/Models/     Core/Services/
|- Data/
|- Resources/
`- UI/Converters/ Helpers/ Models/ Services/ ViewModels/ Views/

tests/
|- 5W2H.Tests.csproj
|- BackupServiceTests.cs
`- TaskServiceTests.cs
```

## Regras de codigo

| Area | Regra |
|------|-------|
| `Core` | Modelos, enums, regras, DTOs, servicos |
| `Data` | SQLite, Dapper, repositorios |
| `UI/ViewModels` | Estado MVVM e comandos |
| `UI/Views` | XAML e code-behind minimo |
| `UI/Services` | Dialogos, arquivos, temas, updates |
| `tests` | xUnit; testes deterministas |

Convenções:

- C# nullable habilitado
- Async com sufixo `Async`
- DI por construtor + `ArgumentNullException`
- Campos privados `_camelCase`
- Strings default `string.Empty`
- ViewModels `partial` + `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`
- Codigo em ingles; UI em portugues

Nao introduzir sem motivo claro: EF Core, nova UI framework, nova lib de graficos, SQL em ViewModel, WPF em `Core`.

## Mudancas

- Checar `git status` antes de editar
- Nao reverter mudancas do usuario
- Escopo pequeno; sem refactor lateral
- Contrato publico mudou: atualizar call sites
- Estrutura duravel mudou: atualizar AGENTS.md e memorias Serena

## Validacao

| Escopo | Comando |
|--------|---------|
| Codigo | `dotnet build` + `dotnet test` |
| UI/XAML | `dotnet build`; `dotnet run --project src/5W2H.App.csproj` se viavel |
| Release | `dotnet publish -c Release src/5W2H.App.csproj -o publish` |
| Docs | Revisar paths e comandos |

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
