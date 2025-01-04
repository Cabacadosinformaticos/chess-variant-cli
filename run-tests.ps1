# Runs every input/output test in iotests/ against a Release build of the game.
# Each iotests/NN-name.in is fed to the program, with iotests/ as the working directory,
# and its output is compared, byte by byte, with iotests/NN-name.out.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

dotnet build src/Project/Project.csproj -c Release -nologo -v q | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Host 'Build failed'; exit 1 }
$app = Join-Path $PSScriptRoot 'src/Project/bin/Release/net8.0/Project.dll'

$passed = 0; $failed = 0
foreach ($input in Get-ChildItem iotests -Filter *.in | Sort-Object Name) {
    $expectedPath = [IO.Path]::ChangeExtension($input.FullName, '.out')
    $actualPath = [IO.Path]::GetTempFileName()

    # Run the program from the iotests folder, so the save files the tests write land there.
    Push-Location (Join-Path $PSScriptRoot 'iotests')
    cmd /c "dotnet `"$app`" < `"$($input.Name)`" > `"$actualPath`""
    Pop-Location

    $expected = [IO.File]::ReadAllBytes($expectedPath)
    $actual = [IO.File]::ReadAllBytes($actualPath)
    $same = ($expected.Length -eq $actual.Length) -and
            (-not (Compare-Object $expected $actual -SyncWindow 0))

    if ($same) {
        Write-Host "PASS  $($input.BaseName)"
        $passed++
    } else {
        Write-Host "FAIL  $($input.BaseName)"
        $failed++
    }
    Remove-Item $actualPath
}

# The tests write save files with relative names; remove everything that is not a test file.
Get-ChildItem (Join-Path $PSScriptRoot 'iotests') -Filter *.txt -File | Remove-Item

Write-Host "$passed passed, $failed failed"
if ($failed -gt 0) { exit 1 }
