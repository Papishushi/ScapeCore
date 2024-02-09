# Function to print a progress bar
function Print-Progress {
    param(
        [int]$progress
    )

    $length = 50
    $barLength = [math]::Round($progress * $length / 100)
    $spacesLength = $length - $barLength

    Write-Host -NoNewline "`r["
    Write-Host -NoNewline ("#" * $barLength)
    Write-Host -NoNewline (" " * $spacesLength)
    Write-Host ("] {0}%" -f $progress)
}

# Clone the main repository
if (-not (Test-Path "ScapeCore")) {
    git clone "https://github.com/Papishushi/ScapeCore"
}

# Check if the clone was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Unable to clone the repository."
    exit 1
}

# Move into the cloned directory
Set-Location "ScapeCore\Core"

# Update and initialize submodules with progress indicator
$totalSubmodules = $args.Count
$currentSubmodule = 0

# Update and initialize submodules
foreach ($submodulePath in $args) {
    $currentSubmodule++
    $progress = [math]::Round($currentSubmodule * 100 / $totalSubmodules)

    # Print progress bar
    Print-Progress $progress

    # Check if module path does not exist or is empty
    if (-not (Test-Path $submodulePath -PathType Container) -or -not (Get-ChildItem $submodulePath)) {
        $env:submodule_path = $submodulePath
        git submodule update --init $env:submodule_path

		$env:proj_items_path = "./Core.projitems"
        dotnet restore $env:proj_items_path

        # Update the .projitems file with the necessary dependencies from the submodule
        python projitems-updater.py

        # Add xmlns to the Project tag
        (Get-Content $env:proj_items_path) | ForEach-Object {
            $_ -replace '<Project>', '<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">'
        } | Set-Content $env:proj_items_path
    }
    else {
        Write-Host "Warning: The submodule directory '$submodulePath' already exists."
        Write-Host "Skipping submodule initialization for '$submodulePath'."
    }
}


Get-ChildItem -Recurse | Where-Object { $_.PSIsContainer -and @(Get-ChildItem $_.FullName) -eq $null } | Remove-Item
dotnet restore "./Core.csproj"
./BuildinTypesUpdater.ps1
Set-Location ../..