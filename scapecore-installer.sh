#!/bin/bash

# Function to print a progress bar
print_progress() {
    local progress=$1
    local length=50
    local bar_length=$((progress * length / 100))
    local spaces_length=$((length - bar_length))

    printf "\r["
    printf "%${bar_length}s" "" | tr ' ' '#'
    printf "%${spaces_length}s" ""
    printf "] %d%%\n" "$progress"
}

# Clone the main repository
if [ ! -d "ScapeCore" ]; then
    git clone "https://github.com/Papishushi/ScapeCore"
fi

# Check if the clone was successful
if [ $? -ne 0 ]; then
    echo "Error: Unable to clone the repository."
    exit 1
fi

# Move into the cloned directory
cd ScapeCore/Core

# Update and initialize submodules with progress indicator
total_submodules=$#
current_submodule=0

# Update and initialize submodules
while [ "$#" -gt 0 ]; do
    current_submodule=$((total_submodules - $# + 1))
    export submodule_path="$1"
    # Print progress bar
    progress=$((current_submodule * 100 / total_submodules))
    # Check if module path does not exist or is empty
    if [ ! -d "$submodule_path" ] || [ -z "$(ls -A "$submodule_path")" ]; then
        print_progress "$progress"
        git submodule update --init "$submodule_path"
		dotnet restore "$submodule_path"/"ScapeCore.Core.$submodule_path.csproj"
        # Update the .projitems file with the necessary dependencies from the submodule
        export proj_items_path="./Core.projitems"
        # Add xlmns to the Project tag
        sed -i 's|<Project>|<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">|g' "$proj_items_path"
    else
        echo "Warning: The submodule directory '$submodule_path' already exists."
        echo "Skipping submodule initialization for '$submodule_path'."
    fi
    shift
done
find . -type d -empty -delete
dotnet restore "./Core.csproj"
./BuildinTypesUpdater.sh
