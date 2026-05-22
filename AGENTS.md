# AGENTS.md — 5W2H

**Modo padrão:** `caveman full`. Linguagem normal em código, commits, PRs e erros.

## Projeto

Stack: `.NET 10`, WPF, CommunityToolkit.Mvvm, SQLite/Dapper, OxyPlot, DI, Velopack.  
App: `src/5W2H.App.csproj` · Testes: `tests/5W2H.Tests.csproj` · Namespace: `FiveW2H.App`  
Fonte de verdade: código, `.csproj`, `.sln`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`.

## Ferramentas

- **Serena MCP:** `check_onboarding_performed` → ler memórias → preferir símbolos/refs antes de arquivo inteiro → gravar só fatos duráveis.
- **Context7:** `npx ctx7@latest library <nome> "<pergunta>"` → escolher ID → `npx ctx7@latest docs <id> "<pergunta>"`. Usar para qualquer lib/SDK externo.
- **RTK:** usar se disponível; declarar `rtk: indisponivel` se ausente.

## Estrutura

```
src/  Application/  Core/  Data/  Infrastructure/  Resources/  UI/{ViewModels,Views,Services}/
tests/  BackupServiceTests.cs  TaskServiceTests.cs
```

## Regras

| Camada | Dono |
|--------|------|
| `Core` | modelos, enums, regras puras — sem deps externas |
| `Application` | casos de uso, DTOs, contratos |
| `Data` | SQLite, Dapper, repositórios |
| `Infrastructure` | import/export, settings, updates |
| `UI/ViewModels` | estado MVVM, comandos — sem SQL |
| `UI/Views` | XAML + code-behind mínimo |
| `UI/Services` | diálogos WPF, interação de tela |

- Nullable habilitado; `async` com sufixo `Async`; DI por construtor + `ArgumentNullException.ThrowIfNull`.
- Campos `_camelCase`; strings `string.Empty`; ViewModels `partial` + `ObservableObject` + `[ObservableProperty]` + `[RelayCommand]`.
- Código em inglês; UI em português.
- Não introduzir: EF Core, nova UI/gráficos lib, SQL em ViewModel, WPF em `Core`.

## Mudanças

- `git status` antes de editar. Não reverter mudanças do usuário.
- Escopo pequeno, sem refactor lateral. Mudou contrato público → atualizar call sites.
- Mudança durável → atualizar este `AGENTS.md` + memórias Serena.
- Validar: `dotnet build` + `dotnet test`. Run se viável; publish com `-c Release`.

## Commit

Ao finalizar alterações, **antes de commitar**, perguntar ao usuário:

> "Faço o commit agora? (sim/não)"

Se sim, garantir que está na branch de desenvolvimento (nunca em `main`/`master`) e commitar no estilo caveman:

```bash
git checkout dev   # ou a branch de desenvolvimento ativa
git add -p         # revisar o que vai no commit
git commit -m "fez X, corrigiu Y, adicionou Z"
```

Mensagem: direta, sem prefixos de convenção (`feat:`, `fix:` etc.).  
Se a branch de dev não existir ou estiver incerta, perguntar antes de criar ou trocar.

## Handoff

```markdown
## Handoff

| Campo     | Valor |
|-----------|-------|
| status    | ok/parcial/bloqueado |
| pedido    | ... |
| feito     | ... |
| pendente  | ... |
| arquivos  | `path` |
| contrato  | sem_mudanca / mudou → <o que mudou> |
| serena    | ok/indisponivel |
| rtk       | ok/indisponivel |
| caveman   | full/lite/ultra/off |

| Comando          | Resultado |
|------------------|-----------|
| `dotnet build`   | ✅/❌/⏭ |
| `dotnet test`    | ✅/❌/⏭ |
| `dotnet run`     | ✅/❌/⏭ |
| `dotnet publish` | ✅/❌/⏭ |

 contexto gasto -> porcentagem/quantidade | Tokens gastos -> quantidade

**Riscos / pendências:** nenhum
```