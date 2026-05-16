# AGENTS.md — 5W2H Management

Guia operacional para agentes. Prosa curta (caveman **lite**). Código/commits/PRs: linguagem normal.

## Snapshot

| Campo | Valor |
|-------|--------|
| App | Desktop WPF — ações 5W2H (CRUD, filtros, backup, import/export, gráficos) |
| Stack | `.NET 8` · WPF · MVVM Toolkit · SQLite + Dapper · OxyPlot · DI · Velopack |
| Layout | Monolito modular → `src/5W2H.App` + `tests/5W2H.Tests` |
| Não recriar | `Domain/` · `Application/` · `Infrastructure/` · `Presentation.WPF` |

## Fonte de verdade

| Prioridade | Regra |
|------------|--------|
| 1 | Código + `.csproj` > docs |
| 2 | Commits úteis: `dcb84f1` · `2cd4243` (simplificação arquitetura) |

## Comunicação (caveman)

| Nível | Quando | Estilo |
|-------|--------|--------|
| **lite** (default aqui) | Handoff, status, listas | Sem filler; frases curtas; tabelas > parágrafo |
| **full** | User pede `/caveman` ou "menos tokens" | Sem artigos; fragmentos OK; termos técnicos exatos |
| **ultra** | User pede `/caveman ultra` | Máxima compressão; setas `→`; abrev. só em prosa |
| **off** | Segurança, ação irreversível, passos ambíguos | Prosa clara; retomar caveman depois |

Código, diff, mensagens de erro, nomes de API/símbolo: **nunca** abreviar.

## Fluxo SERENA (não trivial)

| # | Ação |
|---|------|
| 1 | Ativar projeto SERENA |
| 2 | `check_onboarding_performed` |
| 3 | Ler memórias: `project_overview` · `style_and_conventions` · `suggested_commands` · `task_completion_checklist` |
| 4 | Símbolos primeiro: `get_symbols_overview` → `find_symbol` → `find_referencing_symbols` (antes de mudar contrato público) |
| 5 | `search_for_pattern` para XAML / Markdown / nome incerto |
| 6 | Arquivo inteiro só se símbolo não bastar |

SERENA indisponível → declarar no handoff antes de seguir.

## Estrutura

```text
src/5W2H.App/
|- Core/Models/     Core/Services/
|- Data/
|- Resources/       (ModernTheme.xaml)
`- UI/Converters/ Models/ Services/ ViewModels/ Views/
tests/5W2H.Tests/
```

## Limites (camada → responsabilidade)

| Pasta | Pode | Não |
|-------|------|-----|
| `Core/Models` | Entidades, enums | UI, WPF |
| `Core/Services` | Regras, DTOs, `TaskService`, `BackupService`, import/export | UI |
| `Data` | SQLite, Dapper, repos | Lógica de tela |
| `UI/ViewModels` | Estado MVVM, comandos | SQL direto |
| `UI/Views` | XAML, code-behind mínimo | Regras de negócio |
| `UI/Services` | Dialogs, arquivos, mensagens | — |
| `UI/Converters` | Conversores | — |
| `tests/` | xUnit, AAA, Moq se fizer sentido | — |

**Proibido sem motivo claro:** EF Core (persistência) · nova UI framework · nova lib de gráficos · banco no ViewModel · controle WPF em `Core`.

## Convenções

| Tópico | Regra |
|--------|--------|
| Namespace | `_5W2H.App` |
| Nullability | Habilitado |
| Async | Sufixo `Async` |
| DI | Construtor + `ArgumentNullException` |
| Campos | `_camelCase` |
| Strings default | `string.Empty` |
| ViewModels | `partial` · `ObservableObject` · `[ObservableProperty]` · `[RelayCommand]` |
| Nomes código | Inglês |
| Texto UI | Português (salvo pedido contrário) |
| Docs XML | Tipos/membros públicos importantes em `Core` |

## Roteamento rápido

| Caminho | Foco |
|---------|------|
| `AGENTS.md` `README.md` `ARCHITECTURE.md` `BUILD.md` `DECISIONS.md` `*.sln` `*.csproj` | Docs/build alinhados ao projeto único |
| `src/5W2H.App/Core/**` | Regras, DTOs, serviços |
| `src/5W2H.App/Data/**` | SQL parametrizado, repos |
| `src/5W2H.App/UI/**` | Bindings, comandos, OxyPlot, tema |
| `tests/**` | Testes deterministas |

## Disciplina de mudança

- Checar worktree antes de editar
- Não reverter mudanças do usuário
- Escopo pequeno; atualizar call sites se contrato público mudar
- Código ≠ doc → confiar no código
- Memória SERENA só para fato durável do projeto

## Verificação

| Escopo | Comando |
|--------|---------|
| C# / serviços | `dotnet build` + `dotnet test` |
| XAML / UI | `dotnet build`; `dotnet run` se viável |
| Só testes | `dotnet test` |
| Release | `dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish` |
| Só docs | Revisar caminhos e comandos |

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/5W2H.App/5W2H.App.csproj
dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish
```

Não validou → campo `validacao` no handoff = `nao_executada` + motivo.

---

## Saída obrigatória (handoff)

Ao terminar tarefa (ou parar por bloqueio), responder com **este bloco preenchido**. Copiar estrutura; linhas vazias = omitir seção.

```markdown
## Handoff

| Campo | Valor |
|-------|--------|
| status | `ok` \| `parcial` \| `bloqueado` |
| pedido | (1 linha — o que user pediu) |
| feito | (bullets curtos — mudanças reais) |
| arquivos | `path` · `path` |
| contrato | `sem_mudanca` \| `mudou` → listar símbolos/call sites atualizados |
| serena | `ok` \| `indisponivel` |
| caveman | `lite` \| `full` \| `ultra` \| `off` |

### Validação

| Comando | Resultado |
|---------|-----------|
| `dotnet build` | `pass` \| `fail` \| `skip` |
| `dotnet test` | `pass` \| `fail` \| `skip` (+ N passed / M failed se rodou) |
| `dotnet run` | `pass` \| `fail` \| `skip` |
| publish | `pass` \| `fail` \| `skip` |

### Riscos / pendências

- (ou `nenhum`)

### Próximo (se parcial/bloqueado)

- (ação concreta)
```

### Regras do handoff

| Regra | Detalhe |
|-------|---------|
| Dados > narrativa | Tabelas e paths literais; evitar parágrafo longo |
| Falha | Colar trecho relevante do erro (não resumir de memória) |
| UI | Se tocou XAML: citar binding/comando verificado ou `nao_verificado_runtime` |
| Escopo | Listar só arquivos tocados ou lidos com impacto na decisão |

### Exemplo (ok)

```markdown
## Handoff

| Campo | Valor |
|-------|--------|
| status | ok |
| pedido | Corrigir teste BackupService |
| feito | Ajuste mock path · assert async |
| arquivos | `tests/5W2H.Tests/BackupServiceTests.cs` |
| contrato | sem_mudanca |
| serena | ok |
| caveman | lite |

### Validação

| Comando | Resultado |
|---------|-----------|
| `dotnet build` | pass |
| `dotnet test` | pass — 42 passed, 0 failed |
| `dotnet run` | skip |
| publish | skip |

### Riscos / pendências

- nenhum
```
