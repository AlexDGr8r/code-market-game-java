<#
.SYNOPSIS
  Re-extract the API from the installed game and regenerate the Java stubs. Run after a game update.
#>
param([string]$GameDir)
. (Join-Path $PSScriptRoot 'common.ps1')

$managed = Get-ManagedDir $GameDir
$dll = Build-Tool (Join-Path $PSScriptRoot 'extractor\Extract.cs')
$json = Join-Path $PSScriptRoot '..\api\code-market-api.json'
& dotnet $dll $managed $json
if ($LASTEXITCODE -ne 0) { throw 'Extraction failed' }

& python (Join-Path $PSScriptRoot 'generate.py')
if ($LASTEXITCODE -ne 0) { throw 'Generation failed' }
