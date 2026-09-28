param([switch]$Demo)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    $sdkPath = if ($dotnetCommand) { $dotnetCommand.Source } else { Join-Path $PSScriptRoot '.tools/dotnet/dotnet.exe' }
    if (-not (Test-Path -LiteralPath $sdkPath)) { throw 'Instale el SDK .NET 8 para Windows.' }
    & $sdkPath build KovaqsUI.sln -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación.' }
    & $sdkPath run --project tests/KovaqsUI.SmokeTests -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas.' }
    if ($Demo) { & $sdkPath run --project demo/KovaqsUI.Demo -c Release --no-build }
}
finally { Pop-Location }
