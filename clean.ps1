[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$KeepBuildOutputs,
    [switch]$ResetPackage
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
$project = 'Rhino2Straus'
if (-not (Test-Path -LiteralPath (Join-Path $root ($project + '.sln')))) {
    throw "Project solution not found in $root"
}

# Never follow junctions/symlinks or remove anything outside this project.
function Assert-GeneratedPath([string]$Path) {
    $full = [IO.Path]::GetFullPath($Path)
    if (-not $full.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Cleanup target outside project: $full"
    }
    $current = Get-Item -LiteralPath $full -Force
    while ($null -ne $current) {
        if ($current.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Linked cleanup path: $($current.FullName)" }
        if ($current.FullName -eq $root) { break }
        $current = Get-Item -LiteralPath (Split-Path $current.FullName -Parent) -Force
    }
    if (Test-Path -LiteralPath $full -PathType Container) {
        $links = @(Get-ChildItem -LiteralPath $full -Force -Recurse |
            Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })
        if ($links.Count) { throw "Linked content inside cleanup target: $full" }
    }
}

$targets = [Collections.Generic.List[string]]::new()
if (-not $KeepBuildOutputs) {
    $pending = [Collections.Generic.Stack[string]]::new()
    $pending.Push($root)
    while ($pending.Count) {
        foreach ($dir in Get-ChildItem -LiteralPath $pending.Pop() -Directory -Force) {
            if ($dir.Attributes -band [IO.FileAttributes]::ReparsePoint) { continue }
            if ($dir.FullName -eq (Join-Path $root 'bin')) { continue }
            if ($dir.Name -in '.git', '.vs', '.local', '.work', 'artifacts') { continue }
            if ($dir.Name -in 'bin', 'obj') { $targets.Add($dir.FullName) }
            else { $pending.Push($dir.FullName) }
        }
    }
}

$artifacts = Join-Path $root 'artifacts'
$currentArchive = $project + '.zip'
if ($project -eq 'Rhino2Straus') {
    [xml]$props = Get-Content -LiteralPath (Join-Path $root 'Directory.Build.props') -Raw
    $currentArchive = $project + '-' + $props.Project.PropertyGroup.Version + '.zip'
}
if (Test-Path -LiteralPath $artifacts) {
    $archivePattern = '^' + [regex]::Escape($project) + '(?:-[0-9]+(?:\.[0-9]+)*(?:-[A-Za-z0-9.-]+)?)?\.zip$'
    foreach ($archive in Get-ChildItem -LiteralPath $artifacts -File) {
        if ($archive.Name -match $archivePattern -and $archive.Name -ne $currentArchive) { $targets.Add($archive.FullName) }
    }
    # The old extracted distribution duplicates the plugin now published in bin.
    $legacyPackage = Join-Path $artifacts $project
    if (Test-Path -LiteralPath $legacyPackage) { $targets.Add($legacyPackage) }
}
$package = Join-Path $root 'bin'
if ($ResetPackage -and (Test-Path -LiteralPath $package)) { $targets.Add($package) }
# Validate every resolved absolute target before the first recursive removal.
foreach ($target in $targets) { Assert-GeneratedPath $target }
$removed = 0
foreach ($target in $targets) {
    if ($PSCmdlet.ShouldProcess($target, 'Remove generated build output')) {
        Remove-Item -LiteralPath $target -Recurse -Force
        $removed++
    }
}
Write-Output "$project cleanup: $removed generated paths removed."