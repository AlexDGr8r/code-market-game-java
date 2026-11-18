# Shared helpers for the tools scripts: find the game, and build a C# tool with the .NET Framework
# compiler so it can run on the installed .NET runtime (no .NET SDK needed).

$ErrorActionPreference = 'Stop'

$DefaultGameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Code Market'

function Get-ManagedDir([string]$GameDir) {
    if (-not $GameDir) { $GameDir = $env:CODE_MARKET_DIR }
    if (-not $GameDir) { $GameDir = $DefaultGameDir }
    $managed = Join-Path $GameDir 'Code Market_Data\Managed'
    if (-not (Test-Path (Join-Path $managed 'JavaMarket.Java.dll'))) {
        throw "Can't find Code Market at '$GameDir'. Pass -GameDir or set CODE_MARKET_DIR."
    }
    return $managed
}

function Build-Tool([string]$Source) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { throw "Can't find the .NET Framework C# compiler at $csc" }
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet (a .NET 8 or newer runtime) is required' }

    $outDir = Join-Path $PSScriptRoot '..\target\tools'
    New-Item -ItemType Directory -Force $outDir | Out-Null
    $name = [IO.Path]::GetFileNameWithoutExtension($Source)
    $dll = Join-Path $outDir "$name.dll"
    if (-not (Test-Path $dll) -or (Get-Item $Source).LastWriteTimeUtc -gt (Get-Item $dll).LastWriteTimeUtc) {
        & $csc -nologo -target:exe "-out:$dll" $Source
        if ($LASTEXITCODE -ne 0) { throw "Failed to compile $Source" }
        # The game assemblies target netstandard 2.1, so run on .NET 8+ rather than .NET Framework.
        Set-Content -Encoding ascii (Join-Path $outDir "$name.runtimeconfig.json") `
            '{"runtimeOptions":{"tfm":"net8.0","rollForward":"Major","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}'
    }
    return $dll
}
