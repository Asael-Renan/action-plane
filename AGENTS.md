# AGENTS.md - 5W2H Management

Guia operacional curto para agentes neste repositório.

## Snapshot

- App desktop WPF para gerenciar acoes 5W2H.
- Stack: `.NET 8`, WPF, MVVM Toolkit, SQLite + Dapper, OxyPlot.
- Arquitetura atual: monolito modular em `src/5W2H.App`.
- Nao recriar a arquitetura antiga com `Domain/Application/Infrastructure/Presentation.WPF`.

## Fonte de verdade

- Codigo e `.csproj` vencem documentacao quando houver conflito.
- Considere historico relevante:
  - `dcb84f1 simplificando arquitetura`
  - `2cd4243 simplificando arquitetura`

## Fluxo obrigatorio com SERENA

Para trabalho nao trivial:

1. Ativar o projeto no SERENA.
2. Checar onboarding.
3. Ler as memorias:
   - `project_overview`
   - `style_and_conventions`
   - `suggested_commands`
   - `task_completion_checklist`
4. Preferir ferramentas simbolicas:
   - `get_symbols_overview` para arquivo desconhecido
   - `find_symbol` para tipos/metodos/propriedades
   - `find_referencing_symbols` antes de mudar contrato publico
   - `search_for_pattern` para XAML, Markdown e nomes incertos
5. Ler arquivo inteiro so quando o contexto simbolico nao bastar.

Se SERENA estiver indisponivel, deixar isso explicito antes de seguir.

## Estrutura atual

```text
src/5W2H.App/
|- Core/
|  |- Models/
|  `- Services/
|- Data/
|- Resources/
`- UI/
   |- Converters/
   |- Models/
   |- Services/
   |- ViewModels/
   `- Views/

tests/5W2H.Tests/
```

## Limites arquiteturais

- `Core/Models`: entidades e enums sem dependencia de UI.
- `Core/Services`: regras, validacao, DTOs, resumo, backup, import/export.
- `Data`: SQLite, Dapper, inicializacao e repositorios.
- `UI/ViewModels`: estado MVVM e comandos.
- `UI/Views`: XAML e code-behind minimo.
- `UI/Services`: dialogs, arquivos, mensagens.
- `UI/Converters`: somente conversores.
- `Resources/ModernTheme.xaml`: tema compartilhado.
- `tests/5W2H.Tests`: testes xUnit isolados.

Nao introduzir:

- EF Core para tarefas de persistencia.
- Nova UI framework ou nova lib de graficos sem motivo claro.
- Logica de banco em view model.
- Logica de controle WPF em `Core`.

## Convencoes

- Namespace raiz: `_5W2H.App`
- `Nullable` habilitado
- Metodos async com sufixo `Async`
- Injecao por construtor com `ArgumentNullException`
- Campo privado com `_camelCase`
- Defaults de string com `string.Empty`
- ViewModels `partial` herdando de `ObservableObject`
- Preferir `[ObservableProperty]` e `[RelayCommand]`
- Nomes internos em ingles
- Texto de UI em portugues deve ser preservado, salvo pedido contrario

Documentar com XML comments tipos/interfaces/membros publicos importantes em `Core`.

## Roteamento rapido

- `AGENTS.md`, `README.md`, `ARCHITECTURE.md`, `BUILD.md`, `DECISIONS.md`, `*.sln`, `*.csproj`
  - manter docs e build alinhados com a arquitetura real de projeto unico
- `src/5W2H.App/Core/**/*.cs`
  - preservar regras de negocio, DTOs, `TaskService`, `BackupService`
- `src/5W2H.App/Data/**/*.cs`
  - manter SQL parametrizado e repositorios fora da UI
- `src/5W2H.App/UI/**/*`
  - checar bindings, comandos gerados, layout e compatibilidade com OxyPlot
- `tests/**/*.cs`
  - usar AAA, Moq quando fizer sentido, testes deterministas

## Disciplina de mudanca

- Checar worktree antes de editar.
- Nao reverter mudancas do usuario.
- Manter escopo pequeno e direto.
- Atualizar call sites ao mudar contrato publico.
- Se codigo e docs divergirem, confiar no codigo.
- Atualizar memorias do SERENA so quando um fato duravel do projeto mudar.

## Verificacao

Use a validacao mais estreita possivel.

- C# / servicos: `dotnet build` e `dotnet test`
- XAML / UI: `dotnet build`; rodar o app quando viavel
- Tests only: `dotnet test`
- Release/config de empacotamento: `dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish`
- Docs only: revisar caminhos, comandos e consistencia

Comandos base:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/5W2H.App/5W2H.App.csproj
dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish
```

Se nao conseguir validar, declarar isso no handoff.
