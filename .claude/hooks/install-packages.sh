#!/bin/bash
# NuGet Package Installer for Claude Code Web Sessions
# This script downloads NuGet packages via wget to bypass proxy restrictions
# that prevent dotnet restore from working in web environments.

set -e

# Only run in remote/web sessions
if [ "$CLAUDE_CODE_REMOTE" != "true" ] && [ -z "$CODESPACES" ]; then
    echo "Not a remote session, skipping package installation"
    exit 0
fi

NUGET_CACHE="$HOME/.nuget/packages"
LOCAL_FEED="/tmp/nuget-feed"

mkdir -p "$NUGET_CACHE"
mkdir -p "$LOCAL_FEED"

download_pkg() {
    local name="$1"
    local version="$2"
    local name_lower=$(echo "$name" | tr '[:upper:]' '[:lower:]')
    local pkg_dir="$NUGET_CACHE/$name_lower/$version"
    local nupkg_file="$LOCAL_FEED/$name_lower.$version.nupkg"

    if [ -d "$pkg_dir" ] && [ -f "$nupkg_file" ]; then
        echo "Package $name $version already cached"
        return 0
    fi

    echo "Downloading $name $version..."
    local url="https://api.nuget.org/v3-flatcontainer/$name_lower/$version/$name_lower.$version.nupkg"

    wget -q -O "/tmp/$name_lower.$version.nupkg" "$url" || {
        echo "Failed to download $name $version"
        return 1
    }

    # Extract to cache
    mkdir -p "$pkg_dir"
    unzip -q -o "/tmp/$name_lower.$version.nupkg" -d "$pkg_dir" 2>/dev/null || true

    # Copy to local feed
    cp "/tmp/$name_lower.$version.nupkg" "$nupkg_file"

    # Cleanup
    rm -f "/tmp/$name_lower.$version.nupkg"

    echo "Installed $name $version"
}

echo "Installing NuGet packages for DotNetSDK..."

# ============================================
# ClaudeAgentSDK - Direct dependencies
# ============================================
download_pkg "ProcessX" "1.5.6"
download_pkg "System.Text.Json" "8.0.5"
download_pkg "Microsoft.Extensions.Logging.Abstractions" "8.0.2"

# ProcessX transitive dependencies
download_pkg "System.Threading.Channels" "4.7.0"
download_pkg "System.Threading.Channels" "8.0.0"
download_pkg "System.Runtime.CompilerServices.Unsafe" "6.0.0"

# System.Text.Json transitive dependencies
download_pkg "System.Text.Encodings.Web" "8.0.0"
download_pkg "Microsoft.Bcl.AsyncInterfaces" "8.0.0"

# ============================================
# ClaudeAgentSDK.Tests - Direct dependencies
# ============================================
download_pkg "Microsoft.NET.Test.Sdk" "17.11.1"
download_pkg "xunit" "2.9.2"
download_pkg "xunit.runner.visualstudio" "2.8.2"
download_pkg "coverlet.collector" "6.0.2"
download_pkg "FluentAssertions" "6.12.2"
download_pkg "Moq" "4.20.72"

# xunit transitive dependencies
download_pkg "xunit.core" "2.9.2"
download_pkg "xunit.assert" "2.9.2"
download_pkg "xunit.extensibility.core" "2.9.2"
download_pkg "xunit.extensibility.execution" "2.9.2"
download_pkg "xunit.analyzers" "1.16.0"

# Microsoft.NET.Test.Sdk transitive dependencies
download_pkg "Microsoft.TestPlatform.TestHost" "17.11.1"
download_pkg "Microsoft.TestPlatform.ObjectModel" "17.11.1"
download_pkg "Microsoft.CodeCoverage" "17.11.1"

# Moq transitive dependencies
download_pkg "Castle.Core" "5.1.1"
download_pkg "System.Diagnostics.EventLog" "6.0.0"

# FluentAssertions transitive dependencies
download_pkg "System.Configuration.ConfigurationManager" "6.0.1"

# coverlet transitive dependencies
download_pkg "coverlet.msbuild" "6.0.2"

# xunit transitive dependencies (additional)
download_pkg "xunit.abstractions" "2.0.3"

# System.Configuration.ConfigurationManager transitive dependencies
download_pkg "System.Security.Cryptography.ProtectedData" "4.4.0"
download_pkg "System.Security.Cryptography.ProtectedData" "6.0.0"
download_pkg "System.Security.Permissions" "6.0.0"
download_pkg "System.Security.AccessControl" "6.0.0"
download_pkg "System.Windows.Extensions" "6.0.0"
download_pkg "System.Drawing.Common" "6.0.0"
download_pkg "Microsoft.Win32.SystemEvents" "6.0.0"

# Additional common transitive dependencies
download_pkg "System.Memory" "4.5.5"
download_pkg "System.Buffers" "4.5.1"
download_pkg "System.Numerics.Vectors" "4.5.0"
download_pkg "Microsoft.Extensions.DependencyInjection.Abstractions" "8.0.2"
download_pkg "System.Reflection.Metadata" "1.6.0"
download_pkg "System.Reflection.Metadata" "8.0.0"
download_pkg "System.Collections.Immutable" "8.0.0"
download_pkg "NuGet.Frameworks" "6.11.1"
download_pkg "Newtonsoft.Json" "13.0.1"
download_pkg "Newtonsoft.Json" "13.0.3"

echo ""
echo "Package installation complete!"
echo "Local feed: $LOCAL_FEED"
echo ""
echo "Configure NuGet to use local feed by adding to NuGet.Config:"
echo '  <add key="local-cache" value="/tmp/nuget-feed" />'
