# .NET Version Upgrade

## Strategy
All-at-Once — all 5 projects updated simultaneously to add net9.0 and net10.0 while keeping existing TFMs.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: Single Commit at End
- **Target**: Add net9.0 and net10.0 to all projects; keep netcoreapp3.1;net5.0;net6.0;net7.0;net8.0
- **Source branch**: master
- **Working branch**: upgrade-to-NET10

## Decisions
- Keep all existing TFMs (netcoreapp3.1, net5.0, net6.0, net7.0, net8.0) — user requirement
- All-at-Once strategy — 5 small projects, shallow dependency graph, manageable breaking changes

## Custom Instructions
<!-- Task-specific overrides: "For {taskId}: {instruction}" -->
