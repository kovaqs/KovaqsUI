# Launch Visual Studio with the local SDK, without modifying Windows environment settings.
$ErrorActionPreference = 'Stop'
$locator = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$installation = & $locator -latest -products '*' -requires Microsoft.VisualStudio.Workload.ManagedDesktop -property installationPath
if (-not $installation) { throw 'Visual Studio con desarrollo de escritorio .NET no está instalado.' }
$sdkDirectory = Join-Path $PSScriptRoot '.tools\dotnet'
$useLocalSdk = Test-Path (Join-Path $sdkDirectory 'dotnet.exe')
$previousPath = $env:PATH
$previousRoot = $env:DOTNET_ROOT
try {
    if ($useLocalSdk) {
        $env:DOTNET_ROOT = $sdkDirectory
        $env:PATH = "$sdkDirectory;$previousPath"
    }
    $solution = Join-Path $PSScriptRoot 'KovaUI.sln'
    Start-Process -FilePath (Join-Path $installation 'Common7\IDE\devenv.exe') -ArgumentList ('"' + $solution + '"') -WindowStyle Normal
}
finally {
    $env:PATH = $previousPath
    $env:DOTNET_ROOT = $previousRoot
}

