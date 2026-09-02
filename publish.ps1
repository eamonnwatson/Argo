$version = "1.0"
$build = "{0}{1:000}" -f ((Get-Date).Year % 100), (Get-Date).DayOfYear
$today = (Get-Date).ToString("yyyyMMdd")

$counterFile = ".\publish\.build-counter"
$revision = 0

if (Test-Path $counterFile) {
    $stored = Get-Content $counterFile | ConvertFrom-Json
    if ($stored.date -eq $today) {
        $revision = $stored.revision + 1
    }
}

if (Test-Path .\publish) {
    Remove-Item .\publish\* -Recurse -Force -ErrorAction Stop
}
New-Item -ItemType Directory -Force -Path .\publish | Out-Null

@{ date = $today; revision = $revision } | ConvertTo-Json | Set-Content $counterFile


dotnet publish .\Argo.csproj `
    -c Release `
    -r win-x64 `
    -o .\publish\ `
    -v normal `
    --self-contained true `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    /p:AssemblyVersion=$version.0.0 `
    /p:Version=$version.$build.$revision `
    /p:FileVersion=$version.$build.$revision `
    /p:InformationalVersion="$version.$build (Release $revision)"
    
