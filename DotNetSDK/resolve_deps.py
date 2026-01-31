"""
Dependency Resolution Script for Restricted Environments

This script is a workaround for environments where `dotnet restore` cannot directly access
NuGet.org due to proxy authentication issues (e.g., Claude Code web sessions).
It uses `wget` (which handles the proxy correctly) to download packages to a local feed,
iteratively resolving dependencies by inspecting `.nuspec` files.

Usage:
    python3 resolve_deps.py
"""

import os
import subprocess
import sys
import xml.etree.ElementTree as ET
import glob
import time
import re
import zipfile

NUGET_CACHE = os.path.expanduser("~/.nuget/packages")
LOCAL_FEED = "/tmp/nuget-feed"

os.makedirs(NUGET_CACHE, exist_ok=True)
os.makedirs(LOCAL_FEED, exist_ok=True)

# Set of (name, version) already processed/downloaded
processed_packages = set()

def download_pkg(name, version):
    # Normalize name
    name_lower = name.lower()

    if (name_lower, version) in processed_packages:
        return False

    pkg_dir = os.path.join(NUGET_CACHE, name_lower, version)
    nupkg_file = os.path.join(LOCAL_FEED, f"{name_lower}.{version}.nupkg")

    if os.path.exists(pkg_dir) and os.path.exists(nupkg_file):
        print(f"Package {name} {version} already cached.")
        processed_packages.add((name_lower, version))
        return True

    print(f"Downloading {name} {version}...")
    url = f"https://api.nuget.org/v3-flatcontainer/{name_lower}/{version}/{name_lower}.{version}.nupkg"
    temp_file = f"/tmp/{name_lower}.{version}.nupkg"

    try:
        subprocess.run(["wget", "-q", "-O", temp_file, url], check=True)
    except subprocess.CalledProcessError:
        print(f"Failed to download {name} {version}")
        return False

    try:
        os.makedirs(pkg_dir, exist_ok=True)
        with zipfile.ZipFile(temp_file, 'r') as zip_ref:
            zip_ref.extractall(pkg_dir)

        import shutil
        shutil.copy2(temp_file, nupkg_file)

        os.remove(temp_file)
        print(f"Installed {name} {version}")
        processed_packages.add((name_lower, version))
        return True
    except Exception as e:
        print(f"Error installing {name} {version}: {e}")
        if os.path.exists(temp_file):
            os.remove(temp_file)
        return False

def get_deps_from_nuspec(nuspec_path):
    deps = []
    try:
        tree = ET.parse(nuspec_path)
        root = tree.getroot()
        # Namespace usually exists: {http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd}
        # But we can just use wildcard for namespace or strip it.

        # Helper to find elements regardless of namespace
        def find_all_deps(element):
            found = []
            for child in element:
                if child.tag.endswith("dependency"):
                    found.append(child)
                found.extend(find_all_deps(child))
            return found

        # Parse dependencies
        # <dependencies>
        #   <group targetFramework="...">
        #     <dependency id="X" version="Y" />

        # We'll just grab ALL dependencies for now.
        dependencies_node = None
        for child in root:
             if child.tag.endswith("metadata"):
                 for meta_child in child:
                     if meta_child.tag.endswith("dependencies"):
                         dependencies_node = meta_child
                         break

        if dependencies_node is None:
            return deps

        # Flatten groups
        for child in dependencies_node:
            if child.tag.endswith("group"):
                for dep in child:
                    if dep.tag.endswith("dependency"):
                         id_attr = dep.attrib.get("id")
                         ver_attr = dep.attrib.get("version")
                         if id_attr and ver_attr:
                             deps.append((id_attr, ver_attr))
            elif child.tag.endswith("dependency"):
                 id_attr = child.attrib.get("id")
                 ver_attr = child.attrib.get("version")
                 if id_attr and ver_attr:
                     deps.append((id_attr, ver_attr))

    except Exception as e:
        print(f"Error parsing nuspec {nuspec_path}: {e}")

    return deps

def resolve_transitive_deps(name, version):
    name_lower = name.lower()
    pkg_dir = os.path.join(NUGET_CACHE, name_lower, version)
    nuspec_files = glob.glob(os.path.join(pkg_dir, "*.nuspec"))

    if not nuspec_files:
        print(f"No nuspec found for {name} {version}")
        return

    nuspec = nuspec_files[0]
    deps = get_deps_from_nuspec(nuspec)

    for dep_name, dep_version in deps:
        # Version might be a range like "[1.0.0, )" or "1.0.0".
        # We need to clean it.
        clean_ver = dep_version.strip("[]()")
        if "," in clean_ver:
            # Take the first part if it's min version: "1.0.0, 2.0.0" -> "1.0.0"
            clean_ver = clean_ver.split(",")[0].strip()

        # If verify fails, maybe we need exact version logic, but let's try this.
        if download_pkg(dep_name, clean_ver):
            resolve_transitive_deps(dep_name, clean_ver)

def get_initial_deps():
    deps = []
    for csproj in glob.glob("**/*.csproj", recursive=True):
        try:
            tree = ET.parse(csproj)
            root = tree.getroot()
            for package_ref in root.findall(".//PackageReference"):
                name = package_ref.attrib.get("Include")
                version = package_ref.attrib.get("Version")
                if name and version:
                    deps.append((name, version))
        except Exception as e:
            print(f"Error parsing {csproj}: {e}")
    return deps

def main():
    print("Starting recursive dependency resolution...")

    initial_deps = get_initial_deps()
    print(f"Found {len(initial_deps)} initial dependencies.")

    queue = list(initial_deps)

    for name, version in queue:
        if download_pkg(name, version):
            # If successfully downloaded (or already there), resolve its transitive deps
            resolve_transitive_deps(name, version)

    print("\n--- Running Final Restore Check ---")
    cmd = ["dotnet", "restore", "--source", LOCAL_FEED]
    result = subprocess.run(cmd, capture_output=True, text=True)

    if result.returncode == 0:
        print("Restore succeeded!")
    else:
        print("Restore failed.")
        print(result.stdout)
        print(result.stderr)

        # Fallback: Parse errors if any packages are still missing (maybe because of version mismatch or conditional deps)
        # But hopefully the recursive approach caught them.

if __name__ == "__main__":
    main()
