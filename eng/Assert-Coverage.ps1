[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string[]] $CoveragePath,

    [Parameter(Mandatory)]
    [string[]] $Threshold
)

$ErrorActionPreference = 'Stop'

$requirements = [ordered]@{}
foreach ($entry in $Threshold) {
    if ($entry -notmatch '^(?<assembly>[A-Za-z0-9_.-]+)=(?<line>100|[0-9]{1,2}),(?<branch>100|[0-9]{1,2})$') {
        throw "Invalid threshold '$entry'. Expected Assembly=LinePercent,BranchPercent."
    }

    $assembly = $Matches.assembly
    if ($requirements.Contains($assembly)) {
        throw "Duplicate threshold for assembly '$assembly'."
    }

    $requirements[$assembly] = @{
        Line = [int] $Matches.line
        Branch = [int] $Matches.branch
    }
}

$files = @($CoveragePath | ForEach-Object {
    Get-ChildItem -Path $_ -File -ErrorAction SilentlyContinue
} | Sort-Object -Property FullName -Unique)

if ($files.Count -eq 0) {
    throw 'No Cobertura coverage files matched the supplied path.'
}

$packages = @{}
foreach ($file in $files) {
    $readerSettings = [System.Xml.XmlReaderSettings]::new()
    $readerSettings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $readerSettings.XmlResolver = $null

    $reader = [System.Xml.XmlReader]::Create($file.FullName, $readerSettings)
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
    }
    finally {
        $reader.Dispose()
    }

    $packageNodes = @($document.coverage.packages.package)
    if ($packageNodes.Count -eq 0) {
        throw "Coverage file '$($file.Name)' contains no packages."
    }

    foreach ($package in $packageNodes) {
        $name = [string] $package.name
        if ([string]::IsNullOrWhiteSpace($name)) {
            throw "Coverage file '$($file.Name)' contains a package without an identity."
        }
        if ($packages.ContainsKey($name)) {
            throw "Assembly/package '$name' occurs more than once across coverage input."
        }

        $packages[$name] = $package
    }
}

$failed = $false
foreach ($assembly in $requirements.Keys) {
    if (-not $packages.ContainsKey($assembly)) {
        Write-Error "Required assembly/package '$assembly' is missing from coverage input." -ErrorAction Continue
        $failed = $true
        continue
    }

    [System.Xml.XmlElement] $package = $packages[$assembly]
    $lineNodes = @($package.classes.class | ForEach-Object { $_.lines.line })
    if ($lineNodes.Count -eq 0) {
        throw "Coverage metrics are missing for assembly/package '$assembly'."
    }

    $linesValid = [long] $lineNodes.Count
    $linesCovered = [long] @($lineNodes | Where-Object { [long] $_.hits -gt 0 }).Count
    $branchNodes = @($lineNodes | Where-Object { $_.branch -eq 'True' })
    $branchesValid = 0L
    $branchesCovered = 0L
    foreach ($branch in $branchNodes) {
        if ([string] $branch.'condition-coverage' -notmatch '^\d+(?:\.\d+)?% \((?<covered>\d+)/(?<valid>\d+)\)$') {
            throw "Branch metrics are missing or invalid for assembly/package '$assembly'."
        }
        $branchesCovered += [long] $Matches.covered
        $branchesValid += [long] $Matches.valid
    }

    if ($linesValid -eq 0 -or $branchesValid -eq 0 -or
        $linesCovered -gt $linesValid -or $branchesCovered -gt $branchesValid) {
        throw "Coverage counts are invalid for assembly/package '$assembly'."
    }

    $lineRequirement = $requirements[$assembly].Line
    $branchRequirement = $requirements[$assembly].Branch
    $linePassed = ($linesCovered * 100) -ge ($linesValid * $lineRequirement)
    $branchPassed = ($branchesCovered * 100) -ge ($branchesValid * $branchRequirement)

    $lineDisplay = [math]::Round(($linesCovered * 100.0) / $linesValid, 2)
    $branchDisplay = [math]::Round(($branchesCovered * 100.0) / $branchesValid, 2)
    $result = [string]::Format(
        [Globalization.CultureInfo]::InvariantCulture,
        '{0}: lines {1}/{2} ({3:F2}%, required {4}%); branches {5}/{6} ({7:F2}%, required {8}%)',
        @($assembly, $linesCovered, $linesValid, $lineDisplay, $lineRequirement,
            $branchesCovered, $branchesValid, $branchDisplay, $branchRequirement))
    Write-Output $result

    if (-not $linePassed -or -not $branchPassed) {
        $failed = $true
    }
}

if ($failed) {
    throw 'Coverage requirements were not met.'
}
