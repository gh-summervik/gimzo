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

admin_subdir="admin-cli"
runtime_subdir="runtime-cli"

admin_dir="$target_directory/$admin_subdir"
runtime_dir="$target_directory/$runtime_subdir"

admin_backup="/tmp/gimzo_secrets_admin_$$.json"
runtime_backup="/tmp/gimzo_secrets_runtime_$$.json"

# Backup existing secrets if subdirectories exist
if [ -d "$admin_dir" ] && [ -f "$admin_dir/secrets.json" ]; then
    cp "$admin_dir/secrets.json" "$admin_backup"
fi

if [ -d "$runtime_dir" ] && [ -f "$runtime_dir/secrets.json" ]; then
    cp "$runtime_dir/secrets.json" "$runtime_backup"
fi

# Clean target directory
if [ -d "$target_directory" ]; then
    rm -rf "$target_directory"
fi

mkdir -p "$target_directory"

# Publish Admin CLI
mkdir -p "$admin_dir"
dotnet publish -c Release "$source_directory/Gimzo.Admin.Cli/Gimzo.Admin.Cli.csproj" -o "$admin_dir"
if [ $? -ne 0 ]; then
    echo "Publish Gimzo.Admin.Cli failed."
    exit 1
fi

# Publish Runtime CLI
mkdir -p "$runtime_dir"
dotnet publish -c Release "$source_directory/Gimzo.Runtime.Cli/Gimzo.Runtime.Cli.csproj" -o "$runtime_dir"
if [ $? -ne 0 ]; then
    echo "Publish Gimzo.Runtime.Cli failed."
    exit 1
fi

# Restore secrets
if [ -f "$admin_backup" ]; then
    mv "$admin_backup" "$admin_dir/secrets.json"
fi

if [ -f "$runtime_backup" ]; then
    mv "$runtime_backup" "$runtime_dir/secrets.json"
fi

# Clean up any leftover backups
rm -f "$admin_backup" "$runtime_backup"

exit 0