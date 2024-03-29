# ScapeCore Game Engine [![Codacy Badge](https://app.codacy.com/project/badge/Grade/6f241960c30f4a649ee36cb5323613ca)](https://app.codacy.com/gh/Papishushi/ScapeCore/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

ScapeCore is your go-to free and open-source game engine riding on the MonoGame wave. It's like the Swiss Army knife for game devs – lightweight, modular, and just chill.
In the future, I'm planning on building a GUI Editor also, but for now on, I'm mainly focusing on the Core, Backend, and API.

## Features

- **Modular Design:** Fully modular core, you can select and import the submodules you want to compile for your project. Any user could build their own submodules too and import them on any future project.
- **Event-Driven:** Utilize events for flexible game loop customization.
- **Entity System:** Built-in entity component system through GameObjects and MonoBehaviours, easily extend functionality through a component-based architecture. Create your own Behaviours or Components.
- **Serialize Like a Pro:** Saving and loading game states got you stressed? Chill out with our Serialization Manager. It supports Protocol Buffers and even throws in optional GZip compression – slick and fast.
- **Resource Wizardry:** The Resource Manager is like a magician, juggling resources in a thread-safe show. Load, distribute, and track dependencies effortlessly. Keep your game running smooth and snappy.
- **Scene Management:** Easily add MonoBehaviours or GameObjects to a scene environment, and efficiently organize and transition between game scenes for a seamless player experience.
- **Debugger:** Fully customizable and expandible debugger and command system.
- **Inject your dependencies:** Expand and customize ScapeCore design with your own services and managers, you can load and unload dependencies to ScapeCore at runtime.
- **⚠️2D Physics Engine⚠️:** This one is **WIP**, any contributions would be much appreciated.

## Quick Start

## 1 Installation Methods
#### 1. Using dotnet templates (Recommended for new users)

---

> **Make sure that you have dotnet installed by running the command** **`dotnet --version`. If you don't have .NET SDK installed on your system** **you may want to take a look at [dotnet-install.sh](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install).**

---

On your preferred terminal write down:

      dotnet new install ScapeCore.Templates.CSharp --force

Congratulations! You just installed ScapeCore template package!
Now you can use it as simply as this:

      dotnet new scdesktopgl "your project name"

Once your project is created you can open it on Visual Studio by double-clicking on the .csproj file within the project.

If you want the full developing experience just use the developer template:

      dotnet new devscdesktopgl "your project name"

This template will inform you about post-template creation actions. You must allow it by pressing 'y' and enter.
Once your project is created you can open it on Visual Studio by double-clicking on the .sln file within the project.
#### 2. Using the installer

---

> This installer has two versions, one uses [bash](https://www.gnu.org/software/bash/) and the other [powershell](https://github.com/PowerShell/PowerShell) so it's a prerequisite having a compatible terminal available in your system, if you are working on Windows and want to use bash you may have some problems with this step. For that reason, I recommend you to use [WSL2](https://learn.microsoft.com/es-es/windows/wsl/about) for a seamless integration with the OS. You can use the [default WSL kernel provided by Microsoft](https://learn.microsoft.com/es-es/windows/wsl/install) or [compile your own custom kernel](https://github.com/microsoft/WSL2-Linux-Kernel). If you are going to be using PowerShell you should be good to go on Windows devices.

---

The first step is to download the installer. For this purpose, we can use one of the following methods.

Powershell Users:
- **Download the installer using the CLI**:
  
        New-Item scapecore-installer.ps1  -ItemType File -Value (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/Papishushi/ScapeCore/master/scapecore-installer.ps1')
  
- **Download the installer manually**:
  1.  Navigate to [scapecore-installer.ps1](https://github.com/Papishushi/ScapeCore/blob/master/scapecore-installer.ps1).
  2.  Inside the file display, click on **Download Raw File**.
     
Bash users:
- **Download the installer using wget**:
  
        wget https://raw.githubusercontent.com/Papishushi/ScapeCore/master/scapecore-installer.sh -O scapecore-installer.sh
  
- **Download the installer manually**:
  1.  Navigate to [scapecore-installer.sh](https://github.com/Papishushi/ScapeCore/blob/master/scapecore-installer.sh).
  2.  Inside the file display, click on **Download Raw File**.

Once you have the installer on your PC, you must move it to the directory where you want to locate ScapeCore installation. You can do this by using a command like `mv` or by moving the file manually using your file system.

---

> **Also, make sure that you have dotnet installed by running the command** **`dotnet --version`. If you don't have .NET SDK installed on your system** **you may want to take a look at [dotnet-install.sh](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install).**

---

**Once you are done, you are finally ready to use the installer, the syntax to use the installer is the following:**

    ./scapecore-installer "module name" "another module name" ...

In this example, only the basic ScapeCore is installed:

     ./scapecore-installer

For example, the following instruction would install basic ScapeCore if it is not already installed, as well as the submodules _`Tools`_ and _`Serialization`_ if they haven't been installed already:

     ./scapecore-installer Tools Serialization


#### 3. Using the source code

The first step is to download the source code. For this purpose, we can use one of the following methods.

- **Download the source code using git**:

      git clone https://github.com/Papishushi/ScapeCore.git

- **Download the source code manually**:
  1.  Navigate to [scapecore](https://github.com/Papishushi/ScapeCore/blob/master/).
  2.  Inside the code button, click on **Download Zip**.

Once you have a copy of the source code, you can initialize the submodules by doing the following command:

    git submodule update --init "submodule"

Where "submodule" is the name of the submodule you want to install. If you want to install all submodules, just do not specify a submodule name.

To end the installation process, you must ensure that all dependencies are matched, you can use one of these methods to match dependencies:

- You can manually install all [Dependencies](https://github.com/Papishushi/ScapeCore/network/dependencies) using NuGet.
- You can also match project dependencies using the following `dotnet` command:

      dotnet restore ./Core/Core.csproj

