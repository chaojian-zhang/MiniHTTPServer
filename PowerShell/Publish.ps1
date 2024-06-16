# Bookkeep working directory
$PrevPath = Get-Location
Set-Location $PSScriptRoot

Write-Host "Publish Mini HTTP Server..."
# Delete current data
$PublishFolder = "$PSScriptRoot\..\Publish"
if (Test-Path -Path $PublishFolder) {
    Remove-Item $PublishFolder -Recurse -Force
}

# Publish Executables
$PublishExecutables = @(
    "MiniHTTPServer\MiniHTTPServer.csproj"
)
foreach ($Item in $PublishExecutables)
{
    dotnet publish $PSScriptRoot\..\$Item --use-current-runtime --output $PublishFolder
}

# Validation
$pureExePath = Join-Path $PublishFolder "MiniHTTPServer.exe"
if (-Not (Test-Path $pureExePath)) {
    Write-Host "Build failed."
    Exit
}

# Create archive
$Date = Get-Date -Format yyyyMMdd
$ArchiveFolder = "$PublishFolder\..\Packages"
$ArchivePath = "$ArchiveFolder\MiniHTTPServer_DistributionBuild_B$Date.zip"
New-Item -ItemType Directory -Force -Path $ArchiveFolder
Compress-Archive -Path $PublishFolder\* -DestinationPath $ArchivePath -Force

# Reset working directory
Set-Location $PrevPath