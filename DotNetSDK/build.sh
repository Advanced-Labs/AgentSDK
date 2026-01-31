#!/bin/bash
set -e

# Ensure we are in the script's directory
cd "$(dirname "$0")"

echo "=== 1. Resolving Dependencies ==="
python3 resolve_deps.py

echo "=== 2. Building Solution ==="
dotnet build --no-restore

echo "=== 3. Running Tests ==="
dotnet test --no-build

echo "=== Build and Test Complete ==="
