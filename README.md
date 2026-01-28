Rocks DB
========

[![NuGet](https://img.shields.io/nuget/v/Alma.RocksDb.svg)](https://www.nuget.org/packages/Alma.RocksDb)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Alma.RocksDb.svg)](https://www.nuget.org/packages/Alma.RocksDb)
[![Tests](https://github.com/alma-oss/frocksdb/actions/workflows/tests.yaml/badge.svg)](https://github.com/alma-oss/frocksdb/actions/workflows/tests.yaml)

> Functional overlay for a RocksDB.

## Requirement
Native rocksdb library in your system.

### Following docker file part should be used.

```Dockerfile
###
### Install Rocksdb library
### see: https://github.com/savsgio/docker-rocksdb/blob/main/Dockerfile
###
ARG ROCKSDB_VERSION=v10.9.1
ENV ROCKSDB_VERSION=$ROCKSDB_VERSION
LABEL rocksdb.version=$ROCKSDB_VERSION

RUN apk update \
    && apk --no-cache add \
        binutils \
        # Install bash
        bash \
        bash-completion \
        bash-doc \
        # Install extensions dependencies
        curl \
        git \
        libxml2-dev \
        openldap-dev \
        openssh \
        unzip \
        wget \
        # Rocksdb dependencies
        zlib-dev \
        bzip2-dev \
        lz4-dev \
        snappy-dev \
        zstd-dev \
        gflags-dev \
        build-base \
        linux-headers \
        perl \
    && mkdir /usr/src && \
    cd /usr/src && \
    git clone --depth 1 --branch ${ROCKSDB_VERSION} https://github.com/facebook/rocksdb.git && \
    cd /usr/src/rocksdb && \
    # Fix missing cstdint includes for newer GCC (add to all headers that need it)
    find . \( -name "*.h" -o -name "*.cc" \) -exec grep -l "rocksdb_namespace.h" {} \; | \
      xargs sed -i '/#include "rocksdb\/rocksdb_namespace.h"/a #include <cstdint>' && \
    # Fix 'install -c' flag
    sed -i 's/install -C/install -c/g' Makefile && \
    PORTABLE=1 make -j4 shared_lib && \
    make install-shared \
    && rm -rf /usr/src/rocksdb
### / Rocksdb
```

## Install

Add following into `paket.references`
```
Alma.RocksDb
```

## Release
1. Increment version in `RocksDb.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
