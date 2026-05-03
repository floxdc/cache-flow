# 00.02-update-tfms: Add net9.0 and net10.0 to all project files

## Objective
Add net9.0 and net10.0 to the TargetFrameworks property in all 5 project files while preserving existing TFMs.

## Scope
- CacheFlow/CacheFlow.csproj
- CacheFlow.Json/CacheFlow.Json.csproj
- CacheFlow.MessagePack/CacheFlow.MessagePack.csproj
- CacheFlowTests/CacheFlowTests.csproj
- PerformanceTests/PerformanceTests.csproj

## Done When
All .csproj files contain net9.0 and net10.0 in TargetFrameworks; solution restores cleanly.
