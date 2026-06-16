# AGENTS.md — Alma.RocksDb

## Agent Skills

This repo ships Agent Skill for the `Alma.RocksDb` library. Compatible agents discover it automatically; see `.agents/skills/frocksdb/SKILL.md`.

## Project Purpose

F# library providing a functional overlay for RocksDB — an embedded persistent key-value store. Wraps the `RocksDbSharp` .NET bindings with F#-idiomatic APIs. Published as NuGet package `Alma.RocksDb`.

## Tech Stack

- **Language:** F# (.NET 10)
- **Framework:** .NET SDK library
- **Package management:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **Testing:** Expecto (via `YoloDev.Expecto.TestSdk`)
- **Linting:** fsharplint
- **CI/CD:** GitHub Actions
- **Key dependencies:** `FSharp.Core ~> 10.0`, `RocksDbSharp ~> 6.2`, `RocksDbNative ~> 6.2`, `Microsoft.Extensions.Logging ~> 10.0`

## Commands

```bash
# Install dependencies
dotnet tool restore && dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests

# Lint
dotnet fsharplint lint RocksDb.fsproj
```

## Project Structure

```
frocksdb/
├── RocksDb.fsproj              # Main project (PackageId: Alma.RocksDb, v7.0.1)
├── AssemblyInfo.fs             # Auto-generated
├── src/
│   ├── Utils.fs                # Internal utilities
│   └── RocksDb.fs              # Core RocksDB functional wrapper
├── tests/
│   └── tests.fsproj            # Expecto test project
├── build/
│   └── ...
├── build.sh
├── Dockerfile                  # RocksDB native library build instructions (Alpine)
├── paket.dependencies
├── paket.references            # FSharp.Core, M.E.Logging, RocksDbNative, RocksDbSharp
├── global.json                 # .NET SDK 10.0.0
├── fsharplint.json
├── CHANGELOG.md
└── .github/workflows/
    ├── tests.yaml
    ├── pr-check.yaml
    └── publish.yaml
```

## Prerequisites

**Native RocksDB library required** — RocksDB must be installed on the system. The `Dockerfile` contains build instructions for Alpine Linux:

- Clones RocksDB source (configurable version via `ROCKSDB_VERSION` ARG, default `v10.9.1`)
- Builds with `PORTABLE=1` for shared library
- Installs to system library path
- Patches for newer GCC `cstdint` includes

For local development on macOS: `brew install rocksdb`

## Architecture

Pure library wrapping `RocksDbSharp`:

- **Utils.fs** — internal helpers
- **RocksDb.fs** — functional API over RocksDB operations (open, get, put, delete, iterate)
- Uses `Microsoft.Extensions.Logging` for logging interface

## Build System (FAKE)

Standard library target chain: `Clean → AssemblyInfo → Build → Lint → Tests → Release → Publish`

## CI/CD

- **tests.yaml** — runs on PRs and nightly
- **pr-check.yaml** — blocks fixup commits, runs ShellCheck
- **publish.yaml** — publishes to NuGet on semver tags

## Release Process

1. Increment `<Version>` in `RocksDb.fsproj`
2. Update `CHANGELOG.md`
3. Commit, tag with version, push

## Conventions

- Functional wrapper style — wrap low-level `RocksDbSharp` API with F#-friendly functions
- `Microsoft.Extensions.Logging` for logging abstraction
- Compile order in `.fsproj` matters

## Pitfalls

- **Native dependency** — RocksDB native library must be installed on the system; build will fail without it
- **Dockerfile is for reference** — it documents how to build the native library in Alpine containers, not for running this library
- **Platform-specific** — native library build differs per OS (Alpine uses `apk`, macOS uses `brew`)
- **GCC compatibility** — newer GCC versions need `cstdint` patches (handled in Dockerfile)
- **Paket, not NuGet CLI** — use `dotnet paket install`
- **Test framework** — uses Expecto, not xUnit/NUnit
