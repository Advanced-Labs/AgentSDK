#!/bin/bash
set -e

# Ensure we are in the script's directory
cd "$(dirname "$0")"

echo "=== 1. Restoring Dependencies ==="
dotnet restore

echo "=== 2. Building Solution ==="
dotnet build --no-restore

echo "=== 3. Running Tests ==="
dotnet test --no-build

echo "=== Build and Test Complete ==="
