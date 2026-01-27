# .NET Development Setup (Claude Code Web)

## Problem
In Claude Code web sessions, `dotnet restore` cannot reach NuGet.org due to proxy restrictions. However, `wget` works.

## Solution: Download packages via wget, restore from local feed

### 1. Install .NET SDK (if not present)
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh && chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0  # or check global.json for required version
export PATH="$HOME/.dotnet:$PATH"
```

### 2. Download packages via wget
Use this function to download any NuGet package:
```bash
mkdir -p "$HOME/.nuget/packages" "/tmp/nuget-feed"
download_pkg() {
    local name="$1" version="$2"
    local name_lower=$(echo "$name" | tr '[:upper:]' '[:lower:]')
    wget -q -O "/tmp/$name_lower.$version.nupkg" "https://api.nuget.org/v3-flatcontainer/$name_lower/$version/$name_lower.$version.nupkg"
    mkdir -p "$HOME/.nuget/packages/$name_lower/$version"
    unzip -q -o "/tmp/$name_lower.$version.nupkg" -d "$HOME/.nuget/packages/$name_lower/$version" 2>/dev/null || true
    cp "/tmp/$name_lower.$version.nupkg" "/tmp/nuget-feed/"
    rm -f "/tmp/$name_lower.$version.nupkg"
}
```

### 3. Discover and download all required packages (ITERATIVE PROCESS)

**Step A**: Read `.csproj` files to find `<PackageReference>` entries. Download each one.

**Step B**: Run `dotnet restore --source /tmp/nuget-feed` and check for errors:
- `NU1101: Unable to find package X` → Download package X
- `NU1603: A depends on B (>= version)` → Download B at that version

**Step C**: Repeat Step B until restore succeeds with no errors.

Common transitive dependency chains:
- `System.Text.Json` needs `System.Text.Encodings.Web`
- `xunit` needs `xunit.core`, `xunit.assert`, `xunit.abstractions`, `xunit.extensibility.core`, `xunit.extensibility.execution`, `xunit.analyzers`
- `Microsoft.NET.Test.Sdk` needs `Microsoft.TestPlatform.TestHost`, `Microsoft.TestPlatform.ObjectModel`, `Microsoft.CodeCoverage`, `NuGet.Frameworks`, `Newtonsoft.Json`
- `FluentAssertions` needs `System.Configuration.ConfigurationManager`
- `System.Configuration.ConfigurationManager` needs `System.Security.Cryptography.ProtectedData`, `System.Security.Permissions`, `System.Security.AccessControl`, `Microsoft.Win32.SystemEvents`

### 4. Configure NuGet.Config
Add local feed as primary source in the project's NuGet.Config:
```xml
<packageSources>
  <clear />
  <add key="local-cache" value="/tmp/nuget-feed" />
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
</packageSources>
```

### 5. Build
```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet restore --source /tmp/nuget-feed  # CRITICAL: always use --source flag
dotnet build --no-restore
dotnet test --no-build
```

**CRITICAL**: Always use `--source /tmp/nuget-feed` flag on restore to prevent dotnet from attempting to contact nuget.org (which will fail and hang).
