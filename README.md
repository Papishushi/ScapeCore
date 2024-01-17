# ScapeCore Game Engine [![Codacy Badge](https://app.codacy.com/project/badge/Grade/6f241960c30f4a649ee36cb5323613ca)](https://app.codacy.com/gh/Papishushi/ScapeCore/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

ScapeCore is your go-to free and open-source game engine riding on the MonoGame wave. It's like the Swiss Army knife for game devs – lightweight, modular, and just chill.
In the future I'm planning on building a GUI Editor also, but for now on I'm mainly focusing on the Core, Backend and API.

## Features

- **Modular Design:** Easily extend functionality through a component-based architecture. Create your own Behaviours or Components.
- **Event-Driven:** Utilize events for flexible game loop customization.
- **Entity System:** Built-in entity component system through GameObjects and MonoBehaviours.
- **Serialize Like a Pro:** Saving and loading game states got you stressed? Chill out with our Serialization Manager. It supports Protocol Buffers and even throws in optional GZip compression – slick and fast.
- **Resource Wizardry:** The Resource Manager is like a magician, juggling resources in a thread-safe show. Load, distribute, and track dependencies effortlessly. Keep your game running smooth and snappy.
- **Scene Management:** Easily add MonoBehaviours or GameObjects to a scene environment, and efficiently organize and transition between game scenes for a seamless player experience.

## Quick Start

### Prerequisites

- Install [Dependencies](https://github.com/Papishushi/ScapeCore/network/dependencies).

## 1 Installation Methods

#### 1. Using the installer

---

This installer uses [bash](https://www.gnu.org/software/bash/), so it's a prerequisite having a compatible terminal available in your system, if you are working on Windows you may have some problems with this step. For that reason I recommend you to use [WSL2](https://learn.microsoft.com/es-es/windows/wsl/about) for a seamless integration with the OS. You can use the [default WSL kernel provided by Microsoft](https://learn.microsoft.com/es-es/windows/wsl/install) or [compile your own custom kernel](https://github.com/microsoft/WSL2-Linux-Kernel).

---

The first step is to download the installer. For this purpose we can use one of the following methods.

- **Download the installer using wget**:
  `wget https://raw.githubusercontent.com/Papishushi/ScapeCore/master/scapecore-installer`
- **Download the installer manually**:
  1.  Navigate to [scapecore-installer](https://github.com/Papishushi/ScapeCore/blob/master/scapecore-installer).
  2.  Inside the file display, click on **Download Raw File**.

Once you have the installer on your PC, you must move it to the directory where you want to locate ScapeCore installation. You can do this by using a command like `mv` or by moving the file manually using your file system.

Once you have found a directory for the installation, you can run your terminal and make sure to type the following commands:

    sudo apt update
    sudo apt upgrade

Make sure that you have at least **_python3.12_** by running the following command:

    python --version

If that's not the case use these commands, and press '**_Y_**' when prompted:

    sudo add-apt-repository ppa:deadsnakes/ppa
    sudo apt install python3.12

Once you are done, you are finally ready to use the installer, the syntax to use the installer is the following:

    ./scapecore-installer "module name" "another module name"

In this example only the basic ScapeCore is installed:

     ./scapecore-installer

For example, the following instruction would install basic ScapeCore if it is not already installed, as well as the submodules _`Tools`_ and _`Serialization`_ if they haven't been installed already:

     ./scapecore-installer Tools Serialization


#### 2. Using the source code

The first step is to download the source code. For this purpose we can use one of the following methods.

- **Download the source code using git**:

      git clone https://github.com/Papishushi/ScapeCore.git

- **Download the source code manually**:
  1.  Navigate to [scapecore-installer](https://github.com/Papishushi/ScapeCore/blob/master/scapecore-installer).
  2.  Inside the file display, click on **Download Raw File**.

Once you have a copy of the source code you can initialize the submodules by doing the following command:

    git submodule update --init "submodule"

Where "submodule" is the name of the submodule you want to install. If you want to install all submodules just do not specify a submodule name.

> If you have installed submodules you must manually modify the file _./Core/Core.projitems_ and append a new element under the _\<Compilation>_ tag for each .cs file on the submodule, using the following style: _\<Compile Include="$(MSBuildThisFileDirectory)Serialization/Serialization/Tools/StreamExtensions.cs" />_
