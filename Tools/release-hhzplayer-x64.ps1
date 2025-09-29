
<#

Script that releases mpv.net on GitHub.

Needs 2 positional CLI arguments:
    1. Directory where the mpv.net source code is located.
    2. Directory of the output files, for instance the desktop dir.

Dependencies:
    7zip installation found at: 'C:\Program Files\7-Zip\7z.exe'.
    Inno Setup compiler installation found at: 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'.
    GitHub CLI https://cli.github.com

#>

# Stop when the first error occurs
$ErrorActionPreference = 'Stop'

function DeleteDir($path) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse
    }
}

# Throw error if the file/dir don't exist
function Test($path) {
    if (-not (Test-Path $path)) {
        throw $path
    }
    return $path
}

# Variables
$SourceDir     = Test $args[0]
$OutputRootDir = Test $args[1]

Test (Join-Path $SourceDir 'MpvNet.sln')

$7zFile            = Test 'C:\Program Files\7-Zip\7z.exe'
$InnoSetupCompiler = Test 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'

$ReleaseNotes = "- [.NET Desktop Runtime 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)`n- [Changelog](https://github.com/huhuazhi/mpv.net/blob/main/docs/changelog.md)"
$Repo = 'github.com/huhuazhi/mpv.net'

# Dotnet Publish
$PublishDir64 = Join-Path $SourceDir 'MpvNet.Windows\bin\Release\win-x64\publish\'
$ProjectFile = Test (Join-Path $SourceDir 'MpvNet.Windows\MpvNet.Windows.csproj')
dotnet publish $ProjectFile --self-contained false --configuration Debug --runtime win-x64
$PublishedExeFile64 = Test ($PublishDir64 + 'hhzplayer.exe')

# Create OutputName
$VersionInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($PublishedExeFile64)
$IsBeta = $VersionInfo.FilePrivatePart -ne 0
$BetaString = if ($IsBeta) { '-beta' } else { '' }
$VersionName = $VersionInfo.FileVersion
$OutputName64 = 'hhzplayer-v' + $VersionName + $BetaString + '-portable-x64'

# Create OutputFolder
$OutputDir64   = Join-Path $OutputRootDir ($OutputName64 + '\')
DeleteDir $OutputDir64
mkdir $OutputDir64

# Copy Files
Copy-Item ($PublishDir64 + '*') $OutputDir64
$BinDirX64 = Test (Join-Path $SourceDir 'MpvNet.Windows\bin\Release\')
$ExtraFiles = 'hhzplayer.exe', 'libmpv-2.dll', 'MediaInfo.dll'
$ExtraFiles | ForEach-Object { Copy-Item ($BinDirX64 + $_) ($OutputDir64 + $_) }
$LocaleDir = Test (Join-Path $SourceDir 'MpvNet.Windows\bin\Debug\Locale\')
Copy-Item $LocaleDir ($OutputDir64 + 'Locale') -Recurse

# Pack
$ZipOutputFile64 = Join-Path $OutputRootDir ($OutputName64 + '.zip')
& $7zFile a -tzip -mx9 $ZipOutputFile64 -r ($OutputDir64 + '*')
if ($LastExitCode) { throw $LastExitCode }
Test $ZipOutputFile64

# Inno Setup
''; ''
$InnoSetupScript = Test (Join-Path $SourceDir 'Setup\Inno\inno-setup.iss')
& $InnoSetupCompiler $InnoSetupScript
if ($LastExitCode) { throw $LastExitCode }
$SetupFile = Test (Join-Path $OutputRootDir "hhzplayer-v$VersionName-setup-x64.exe")

if ($IsBeta) {
    $NewSetupFile = Join-Path $OutputRootDir "hhzplayer-v$VersionName-beta-setup-x64.exe"
    Move-Item $SetupFile $NewSetupFile
    $SetupFile = $NewSetupFile
}

# Release
$Title = 'v' + $VersionName + $BetaString

gh release create $Title -t $Title -n $ReleaseNotes --repo $Repo $ZipOutputFile64 $ZipOutputFileARM64 $SetupFile


if ($LastExitCode) { throw $LastExitCode }
