Rocks DB
========

> Functional overlay for a RocksDB.

## Requirement
Native rocksdb library in your system.

### Following docker file part should be used.

```Dockerfile
###
### Install Rocksdb library
### see: https://github.com/savsgio/docker-rocksdb/blob/main/Dockerfile
###
ARG ROCKSDB_VERSION=v7.8.3
ENV ROCKSDB_VERSION=$ROCKSDB_VERSION

RUN apk update && \
    apk add --no-cache \
        zlib-dev bzip2-dev \
        lz4-dev \
        snappy-dev \
        zstd-dev \
        gflags-dev \
    && apk add --no-cache \
        build-base \
        linux-headers \
        git \
        bash \
        perl \
    && mkdir /usr/src && \
    cd /usr/src && \
    git clone --depth 1 --branch ${ROCKSDB_VERSION} https://github.com/facebook/rocksdb.git && \
    cd /usr/src/rocksdb && \
    # Fix 'install -c' flag
    sed -i 's/install -C/install -c/g' Makefile && \
    PORTABLE=1 make -j4 shared_lib && \
    make install-shared && \
    apk del \
        build-base \
        linux-headers \
        git \
        bash \
        perl \
    && rm -rf /usr/src/rocksdb
### / Rocksdb
```

## Install

Add following into `paket.dependencies`
```
git ssh://git@bitbucket.lmc.cz:7999/archi/nuget-server.git master Packages: /nuget/
# LMC Nuget dependencies:
nuget Lmc.RocksDb
```

Add following into `paket.references`
```
Lmc.RocksDb
```

## Use
[todo]

## Release
1. Increment version in `RocksDb.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it
4. Run `$ ./build.sh -t release`
5. Go to `nuget-server` repo, run `./build.sh -t copyAll` and push new versions

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)
- [FAKE](https://fake.build/fake-gettingstarted.html)

### Build
```bash
./build.sh
```
