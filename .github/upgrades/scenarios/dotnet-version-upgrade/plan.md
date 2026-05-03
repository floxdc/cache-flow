# .NET Version Upgrade Plan

## Overview

**Target**: Add net9.0 and net10.0 to all projects while keeping netcoreapp3.1;net5.0;net6.0;net7.0;net8.0
**Scope**: 5 projects (3 libraries, 1 test project, 1 benchmark project)

### Selected Strategy
**All-at-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 5 projects, 2-level dependency graph, mostly TFM additions + package bumps + targeted API fixes.

## Tasks

### 01-sdk-prerequisites: Validate .NET SDK prerequisites

Verify that the .NET 9 and .NET 10 SDKs are installed and that global.json (if present) is compatible with both targets. Ensure the build environment can compile all target frameworks before making project changes.

**Done when**: .NET 9 and .NET 10 SDKs confirmed installed; global.json (if present) allows both versions.

---

### 02-update-tfms: Add net9.0 and net10.0 to all project files

Add net9.0 and net10.0 to the TargetFrameworks property in all five project files: CacheFlow, CacheFlow.Json, CacheFlow.MessagePack, CacheFlowTests, and PerformanceTests. Preserve all existing TFMs.

**Done when**: All .csproj files contain netcoreapp3.1;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0 (or net6.0;net9.0;net10.0 for test-only projects); solution restores cleanly.

---

### 03-update-packages: Update NuGet packages for net9.0/net10.0 compatibility

Update package references across all projects to versions that support the new target frameworks. Address the security vulnerability and deprecated package in CacheFlowTests. Apply recommended version bumps for net9.0/net10.0 targets.

**Done when**: All packages updated to net9.0/net10.0 compatible versions; no security vulnerabilities; no deprecated packages; restore succeeds.

---

### 04-fix-api-issues: Fix source incompatibilities and behavioral changes

Address the Api.0002 source incompatibilities in CacheFlowTests (20 occurrences) and the Api.0003 behavioral changes in CacheFlow (4 occurrences) identified in the assessment. Apply necessary code changes to compile and behave correctly under net9.0 and net10.0.

**Done when**: Solution builds with 0 errors across all target frameworks; all API usages updated or suppressed with documented rationale.

---

### 05-validate: Build and test all target frameworks

Build the full solution targeting all frameworks, run CacheFlowTests, and verify the benchmark project compiles. Confirm no regressions on existing TFMs.

**Done when**: Solution builds with 0 errors; all tests pass on all target frameworks; no regressions on existing TFMs (netcoreapp3.1 through net8.0).
