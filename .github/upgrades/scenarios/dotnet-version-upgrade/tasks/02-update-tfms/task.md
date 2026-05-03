# 02-update-tfms: Add net9.0 and net10.0 to all project files

Add net9.0 and net10.0 to the TargetFrameworks property in all five project files: CacheFlow, CacheFlow.Json, CacheFlow.MessagePack, CacheFlowTests, and PerformanceTests. Preserve all existing TFMs.

**Done when**: All .csproj files contain netcoreapp3.1;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0 (or net6.0;net9.0;net10.0 for test-only projects); solution restores cleanly.
