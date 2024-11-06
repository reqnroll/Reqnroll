param (
  [Parameter(Mandatory)]
  $packagesDir
)

function Start-Section($sectionName) {
  Write-Output ''
  Write-Output "*** $sectionName ***"
}

function Confirm-Exit-Code($commandName, $cmdOutput = "") {
  Write-Debug "$cmdOutput (exit code: $LASTEXITCODE)"
  if ($LASTEXITCODE -ne 0) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = "Yellow"
    Write-Output $cmdOutput
    $host.UI.RawUI.ForegroundColor = "Red"
    Write-Output 'error' "  FAILED: ${commandName} ($LASTEXITCODE)"
    $host.UI.RawUI.ForegroundColor = $fc
    if (-not ($LASTEXITCODE -gt 0)) {
      Exit 100
    }
    Exit $LASTEXITCODE
  }
  Write-Output "$commandName done."
}

function Publish-Packages-To-NuGet($settings) {
  Start-Section 'Publish Packages to NuGet.org'
  $files = Join-Path $settings.OutputDir '*.nupkg' -Resolve
  foreach ($file in $files) {
    Write-Output "Uploading $file"
    Write-Output "dotnet nuget push $file -k $env:NUGET_PUBLISH_KEY -s https://api.nuget.org/v3/index.json --no-symbols --skip-duplicate"
    dotnet nuget push $file -k $env:NUGET_PUBLISH_KEY -s https://api.nuget.org/v3/index.json --no-symbols --skip-duplicate
    Confirm-Exit-Code "upload $file"
  }
}

function Publish-Symbol-Packages-To-NuGet($settings) {
  Start-Section 'Publish Symbol Packages to NuGet.org'
  $files = Join-Path $settings.OutputDir '*.snupkg' -Resolve
  foreach ($file in $files) {
    Write-Output "Uploading $file"
    Write-Output "dotnet nuget push $file -k $env:NUGET_PUBLISH_KEY -s https://api.nuget.org/v3/index.json --skip-duplicate"
    dotnet nuget push $file -k $env:NUGET_PUBLISH_KEY -s https://api.nuget.org/v3/index.json --skip-duplicate
    Confirm-Exit-Code "upload $file"
  }
}

$settings = [PSCustomObject]@{
  OutputDir = $packagesDir
}

Publish-Packages-To-NuGet $settings
Publish-Symbol-Packages-To-NuGet $settings