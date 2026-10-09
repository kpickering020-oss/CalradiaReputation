# CalradiaReputation

CalradiaReputation is a Bannerlord reputation and nickname mod scaffold targeted at the locally installed game on this PC. The project is set up to compile against the game binaries when the Bannerlord install is available on the host machine.

This repository currently contains the Phase 1 foundation: the module manifest, scene setup, persistence classes, and the initial campaign behavior scaffolding required for a persistent reputation system.

## Local verification requirements

Before compiling in Visual Studio, confirm the following paths exist on the local machine:

- D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord
- D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\Modules\
- D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\

The project references the official TaleWorlds assemblies from the local install. If the install path differs, update the `HintPath` entries in the `.csproj` file before building.

## Phase 1 delivered

- Module bootstrap and SubModule registration
- Persistent reputation state and schema versioning
- Campaign behavior scaffolding for save/load sync
- Timeline and career-history model classes
- Nickname data model, registry, evaluator, and claim classes
- Automated tests for core logic and persistence assumptions

## Build steps

1. Open the solution in Visual Studio.
2. Restore NuGet packages.
3. Ensure the Bannerlord DLL references resolve from the local install.
4. Build the solution.
5. Copy the generated `CalradiaReputation.dll` into the Bannerlord `Modules/CalradiaReputation/bin/Win64_Shipping_Client/` folder.
6. Add the module to the game or deploy through the included PowerShell script.

## Notes

This repository intentionally avoids modifying original game files. The content is concentrated in the module itself and the deployment script.
