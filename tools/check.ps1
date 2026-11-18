<#
.SYNOPSIS
  Check Java files with Code Market's own compiler, without starting the game.
.EXAMPLE
  .\tools\check.ps1 examples\src\main\java\Main.java
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true, Position = 0, ValueFromRemainingArguments = $true)][string[]]$Files,
    [string]$GameDir
)
. (Join-Path $PSScriptRoot 'common.ps1')

$managed = Get-ManagedDir $GameDir
$dll = Build-Tool (Join-Path $PSScriptRoot 'checker\Check.cs')
& dotnet $dll $managed @Files
exit $LASTEXITCODE
