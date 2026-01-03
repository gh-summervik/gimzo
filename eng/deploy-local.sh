#!/bin/bash

if [ $# -ne 2 ]; then
    echo "Usage: $0 <source_directory> <target_directory>"
    exit 1
fi

source_directory="$1"
target_directory="$2"

if [ ! -d "$source_directory" ]; then
    echo "$source_directory is not a directory."
    exit 1
fi

secrets_backup="/tmp/gimzo_secrets_$$.json"

if [ -d "$target_directory" ]; then
    if [ -f "$target_directory/secrets.json" ]; then
        cp "$target_directory/secrets.json" "$secrets_backup"
    fi
    rm -rf "$target_directory"
fi

mkdir -p "$target_directory"

# Publish Admin CLI
dotnet publish -c Release "$source_directory/Gimzo.Admin.Cli/Gimzo.Admin.Cli.csproj" -o "$target_directory"
if [ $? -ne 0 ]; then
    echo "Publish Gimzo.Admin.Cli failed."
    exit 1
fi

# Publish Runtime CLI
dotnet publish -c Release "$source_directory/Gimzo.Runtime.Cli/Gimzo.Runtime.Cli.csproj" -o "$target_directory"
if [ $? -ne 0 ]; then
    echo "Publish Gimzo.Runtime.Cli failed."
    exit 1
fi

if [ -f "$secrets_backup" ]; then
    mv "$secrets_backup" "$target_directory/secrets.json"
fi

rm -f "$secrets_backup"

exit 0