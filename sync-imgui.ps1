[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]$BindingsRoot,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$NuGetPackagesRoot
)

$ErrorActionPreference = "Stop"

$packageRoot = $PSScriptRoot
if (-not $BindingsRoot) {
    $BindingsRoot = Join-Path (Split-Path -Parent $packageRoot) "ImGui.NET.4Unity"
}

$BindingsRoot = (Resolve-Path -LiteralPath $BindingsRoot).Path

function Copy-RequiredFile {
    param (
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Source -PathType Leaf)) {
        throw "Required pipeline artifact was not found: $Source"
    }

    $destinationDirectory = Split-Path -Parent $Destination
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    Write-Host "Copied $Destination"
}

$libraries = @(
    @{ Plugin = "imgui";         Managed = "ImGui.NET";        Native = "cimgui"         },
    @{ Plugin = "implot";        Managed = "ImPlot.NET";       Native = "cimplot"        },
    @{ Plugin = "implot3d";      Managed = "ImPlot3D.NET";     Native = "cimplot3d"      },
    @{ Plugin = "imnodes";       Managed = "ImNodes.NET";      Native = "cimnodes"       },
    @{ Plugin = "imnodes_r";     Managed = "ImNodesR.NET";     Native = "cimnodes_r"     },
    @{ Plugin = "imguizmo";      Managed = "ImGuizmo.NET";     Native = "cimguizmo"      },
    @{ Plugin = "imguizmo_quat"; Managed = "ImGuizmoQuat.NET"; Native = "cimguizmo_quat" },
    @{ Plugin = "cimCTE";        Managed = "CimCTE.NET";       Native = "cimCTE"         }
)

$platforms = @(
    @{ Directory = "win-x86";   Extension = "dll"   },
    @{ Directory = "win-x64";   Extension = "dll"   },
    @{ Directory = "win-arm64"; Extension = "dll"   },
    @{ Directory = "linux-x64"; Extension = "so"    },
    @{ Directory = "osx";       Extension = "dylib" }
)

foreach ($library in $libraries) {
    $pluginRoot = Join-Path $packageRoot "Plugins\$($library.Plugin)"
    $managedSource = Join-Path $BindingsRoot "bin\$Configuration\$($library.Managed)\netstandard2.0\$($library.Managed).dll"
    $managedDestination = Join-Path $pluginRoot "$($library.Managed).dll"
    Copy-RequiredFile -Source $managedSource -Destination $managedDestination

    foreach ($platform in $platforms) {
        $nativeSource = Join-Path $BindingsRoot "deps\$($library.Native)\$($platform.Directory)\$($library.Native).$($platform.Extension)"
        $nativeDestination = Join-Path $pluginRoot "$($platform.Directory)\$($library.Native).$($platform.Extension)"
        Copy-RequiredFile -Source $nativeSource -Destination $nativeDestination
    }
}

if (-not $NuGetPackagesRoot) {
    if ($env:NUGET_PACKAGES) {
        $NuGetPackagesRoot = $env:NUGET_PACKAGES
    } else {
        $NuGetPackagesRoot = Join-Path $env:USERPROFILE ".nuget\packages"
    }
}

$imguiProject = Get-Content -LiteralPath (Join-Path $BindingsRoot "src\ImGui.NET\ImGui.NET.csproj") -Raw
$unsafeMatch = [regex]::Match(
    $imguiProject,
    '<PackageReference\s+Include="System\.Runtime\.CompilerServices\.Unsafe"\s+Version="(?<version>[^"]+)"'
)
if (-not $unsafeMatch.Success) {
    throw "Could not determine the System.Runtime.CompilerServices.Unsafe package version."
}

$unsafeVersion = $unsafeMatch.Groups["version"].Value
$unsafeSource = Join-Path $NuGetPackagesRoot "system.runtime.compilerservices.unsafe\$unsafeVersion\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll"
$unsafeDestination = Join-Path $packageRoot "Plugins\System.Runtime.CompilerServices.Unsafe.dll"
Copy-RequiredFile -Source $unsafeSource -Destination $unsafeDestination

Write-Host "UImGui dependency synchronization completed."
