# .NET Version Upgrade Progress

**Progress**: 1/2 tasks complete (50%) ![50%](https://progress-bar.xyz/50)

Adding net9.0 and net10.0 support to 5 projects (CacheFlow, CacheFlow.Json, CacheFlow.MessagePack, CacheFlowTests, PerformanceTests) while preserving all existing target frameworks (netcoreapp3.1 through net8.0). Using All-at-Once strategy.

**Progress**: 0/1 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

   - ✅ 00.01-sdk-prerequisites: Validate .NET SDK prerequisites

- 🔄 00-upgrade: Upgrade all projects to add net9.0 and net10.0
- ?? 02-update-tfms: Add net9.0 and net10.0 to all project files
- ?? 03-update-packages: Update NuGet packages for net9.0/net10.0 compatibility
- ?? 04-fix-api-issues: Fix source incompatibilities and behavioral changes
- ?? 05-validate: Build and test all target frameworks
