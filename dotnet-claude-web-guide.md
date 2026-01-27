# .NET Development in Claude Code Web Sessions

This guide enables Claude Code Web instances to set up .NET development environments and handle NuGet package restoration autonomously.

## The Problem

Claude Code web environments have proxy restrictions that prevent:
1. `dotnet restore` from reaching NuGet.org (dotnet's HTTP client fails proxy auth)
2. .NET SDK may not be pre-installed

However, `wget` and `curl` work correctly with the proxy.

## Solution Overview

1. Install .NET SDK using the official install script (it uses curl internally)
2. Download NuGet packages via wget to bypass proxy restrictions
3. Create a local NuGet feed that dotnet can use offline
4. Build using only the local feed

---

## Step 1: Install .NET SDK

```bash
# Download and run the official .NET install script
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh

# Install LTS version (or specific version if project requires it)
/tmp/dotnet-install.sh --channel LTS

# For a specific version (check global.json in the project):
# /tmp/dotnet-install.sh --channel 8.0

# Add to PATH for current session
export PATH="$HOME/.dotnet:$PATH"

# Verify installation
dotnet --version
```

**Important**: Check if the project has a `global.json` file specifying a required SDK version. Install that version specifically.

---

## Step 2: Create Package Download Script

Create a script that downloads packages via wget. The key function:

```bash
#!/bin/bash
set -e

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

    # Extract to global cache
    mkdir -p "$pkg_dir"
    unzip -q -o "/tmp/$name_lower.$version.nupkg" -d "$pkg_dir" 2>/dev/null || true

    # Copy to local feed
    cp "/tmp/$name_lower.$version.nupkg" "$nupkg_file"

    # Cleanup temp file
    rm -f "/tmp/$name_lower.$version.nupkg"

    echo "Installed $name $version"
}

# Add packages here (see Step 3 for how to discover them)
# download_pkg "PackageName" "Version"
```

---

## Step 3: Discover Required Packages (Iterative Process)

**This is the critical part.** You must iteratively discover all required packages:

### 3a. Find Direct Dependencies

Read the `.csproj` files to find `<PackageReference>` entries:

```xml
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

Add these to your download script:
```bash
download_pkg "System.Text.Json" "8.0.5"
```

### 3b. Discover Transitive Dependencies

Run restore and parse errors to find missing packages:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet restore --source /tmp/nuget-feed 2>&1
```

The output will show errors like:
```
error NU1101: Unable to find package System.Text.Encodings.Web. No packages exist with this id in source(s): local-cache
```

Or version mismatch warnings that become errors with TreatWarningsAsErrors:
```
error NU1603: PackageX depends on PackageY (>= 4.7.0) but PackageY 4.7.0 was not found.
```

### 3c. Iterative Resolution Loop

```
1. Run: dotnet restore --source /tmp/nuget-feed
2. If errors about missing packages:
   - Add missing packages to download script
   - Run download script
   - Go to step 1
3. If no errors: Done!
```

**Common transitive dependency chains to watch for:**

- `System.Text.Json` → `System.Text.Encodings.Web`, `Microsoft.Bcl.AsyncInterfaces`
- `Microsoft.NET.Test.Sdk` → `Microsoft.TestPlatform.*`, `Microsoft.CodeCoverage`, `NuGet.Frameworks`
- `xunit` → `xunit.core`, `xunit.assert`, `xunit.abstractions`, `xunit.extensibility.*`, `xunit.analyzers`
- `FluentAssertions` → `System.Configuration.ConfigurationManager`
- `System.Configuration.ConfigurationManager` → `System.Security.*`, `System.Drawing.Common`, `Microsoft.Win32.SystemEvents`
- `Moq` → `Castle.Core`, `System.Diagnostics.EventLog`

---

## Step 4: Configure NuGet.Config

Update or create `NuGet.Config` in the project to prioritize local feed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <!-- Local cache must be first -->
    <add key="local-cache" value="/tmp/nuget-feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

---

## Step 5: Build and Test

```bash
export PATH="$HOME/.dotnet:$PATH"

# Restore using ONLY local feed (critical - avoids proxy issues)
dotnet restore --source /tmp/nuget-feed

# Build
dotnet build --no-restore

# Test
dotnet test --no-build
```

**Important**: Always use `--source /tmp/nuget-feed` for restore to prevent dotnet from trying to contact nuget.org.

---

## Complete Working Example

Here's how to set up a typical .NET 8 project with xunit tests:

```bash
#!/bin/bash
set -e

# === STEP 1: Install .NET SDK ===
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0
export PATH="$HOME/.dotnet:$PATH"

# === STEP 2: Setup package downloader ===
NUGET_CACHE="$HOME/.nuget/packages"
LOCAL_FEED="/tmp/nuget-feed"
mkdir -p "$NUGET_CACHE" "$LOCAL_FEED"

download_pkg() {
    local name="$1"
    local version="$2"
    local name_lower=$(echo "$name" | tr '[:upper:]' '[:lower:]')
    local pkg_dir="$NUGET_CACHE/$name_lower/$version"
    local nupkg_file="$LOCAL_FEED/$name_lower.$version.nupkg"

    [ -d "$pkg_dir" ] && [ -f "$nupkg_file" ] && return 0

    echo "Downloading $name $version..."
    wget -q -O "/tmp/$name_lower.$version.nupkg" \
        "https://api.nuget.org/v3-flatcontainer/$name_lower/$version/$name_lower.$version.nupkg"

    mkdir -p "$pkg_dir"
    unzip -q -o "/tmp/$name_lower.$version.nupkg" -d "$pkg_dir" 2>/dev/null || true
    cp "/tmp/$name_lower.$version.nupkg" "$nupkg_file"
    rm -f "/tmp/$name_lower.$version.nupkg"
}

# === STEP 3: Download packages (discovered from .csproj and restore errors) ===
# [Add your project's packages here]

# === STEP 4: Restore and Build ===
dotnet restore --source /tmp/nuget-feed
dotnet build --no-restore
dotnet test --no-build
```

---

## Troubleshooting

### "Unable to load the service index for source https://api.nuget.org"
You're not using `--source /tmp/nuget-feed`. Always specify the local source explicitly.

### "Unable to find package X"
The package isn't in your local feed. Add it to the download script and re-run.

### "PackageX depends on PackageY (>= version) but was not found"
You need to download the specific version mentioned. If the project has `TreatWarningsAsErrors`, this becomes a hard error.

### Build works but tests fail to run
Make sure you downloaded all test framework packages including runners (`xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, etc.)

---

## Key Takeaways

1. **wget works, dotnet restore doesn't** - Always download packages via wget
2. **Iterative discovery** - Run restore, find missing packages, download them, repeat
3. **Use --source flag** - Always specify `--source /tmp/nuget-feed` to avoid network calls
4. **Check global.json** - Install the exact SDK version the project requires
5. **Transitive dependencies** - Each package may require many others; discover them iteratively
