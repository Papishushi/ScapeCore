# Function to print a progress bar
function Print-Progress {
    param(
        [int]$progress
    )

    Write-Progress -Activity "Installing ScapeCore" -Status "$progress% Complete:" -PercentComplete $progress
}

# Clone the main repository
if (-not (Test-Path "ScapeCore")) {
    git clone "https://github.com/Papishushi/ScapeCore"
}

# Check if the clone was successful
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: Unable to clone the repository."
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

        # Add xmlns to the Project tag
        (Get-Content $env:proj_items_path) | ForEach-Object {
            $_ -replace '<Project>', '<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">'
        } | Set-Content $env:proj_items_path
    }
    else {
        Write-Information "Warning: The submodule directory '$submodulePath' already exists."
        Write-Information "Skipping submodule initialization for '$submodulePath'."
    }
}

Get-ChildItem -Recurse | Where-Object { $_.PSIsContainer -and @(Get-ChildItem $_.FullName) -eq $null } | Remove-Item
dotnet restore "./Core.csproj"
./BuildinTypesUpdater.ps1
Set-Location ../..
