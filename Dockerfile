FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine

ARG PRIVATE_FEED_USER
ARG PRIVATE_FEED_PASS

ENV PRIVATE_FEED_USER=${PRIVATE_FEED_USER}
ENV PRIVATE_FEED_PASS=${PRIVATE_FEED_PASS}

# Setup Dotnet Core tools global path
ENV PATH="${PATH}:/root/.dotnet/tools"

###
### Install Rocksdb library
### see: https://github.com/savsgio/docker-rocksdb/blob/main/Dockerfile
###
ARG ROCKSDB_VERSION=v9.2.1
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
    # Fix 'install -c' flag
    sed -i 's/install -C/install -c/g' Makefile && \
    PORTABLE=1 make -j4 shared_lib && \
    make install-shared \
    && rm -rf /usr/src/rocksdb
### / Rocksdb

# build scripts
COPY ./build.sh /lib/
COPY ./build /lib/build
COPY ./paket.dependencies /lib/
COPY ./paket.references /lib/
COPY ./paket.lock /lib/

# sources
COPY ./RocksDb.fsproj /lib/
COPY ./src /lib/src
COPY ./tests /lib/tests

# others
COPY ./.git /lib/.git
COPY ./.config /lib/.config

WORKDIR /lib

CMD ["./build.sh", "-t", "tests"]
