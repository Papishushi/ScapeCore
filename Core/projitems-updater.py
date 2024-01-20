import os
# Importing the necessary function from defusedxml
from defusedxml import defuse_stdlib

# Calling defuse_stdlib to patch the standard library
defuse_stdlib()

import xml.etree.ElementTree as eT

submodule_path = os.getenv('submodule_path')
proj_items_path = os.getenv('proj_items_path')

if os.path.isdir(submodule_path):
    submodule_files = [os.path.join(dirpath, filename)
                       for dirpath, _, filenames in os.walk(submodule_path)
                       for filename in filenames
                       if filename.endswith(".cs")]
    if os.path.exists(proj_items_path):
        eT.register_namespace('xmlns', 'http://schemas.microsoft.com/developer/msbuild/2003')
        namespace_map = {"xmlns": "http://schemas.microsoft.com/developer/msbuild/2003"}

        tree = eT.parse(proj_items_path)
        root = tree.getroot()

        files_tag = root.find(".//xmlns:ItemGroup", namespaces=namespace_map)

        # Check if the tag is found
        if files_tag is not None:
            for file in submodule_files:
                file_element = eT.Element("Compile", Include="\\$(MSBuildThisFileDirectory)" + file)
                files_tag.append(file_element)

            # Remove the ns0: prefix from the root element and its descendants
            for elem in root.iter():
                if '}' in elem.tag:
                    elem.tag = elem.tag.split('}', 1)[1]

            eT.indent(tree, space="  ", level=0)
            tree.write(proj_items_path, encoding='utf-8', xml_declaration=True)

            print(f"\nSubmodule files added to {proj_items_path}")
        else:
            print("Error: <Compilation> tag not found in the .projitems file structure.")
    else:
        print(f"Error: {proj_items_path} not found.")
else:
    print(f"Submodule not found. No changes made to {proj_items_path}")
