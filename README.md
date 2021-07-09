# DataTransfer Legacy SDK

This is a port of old WebAPI Import API to Kepler API


## Requirements
.NET Core SDK 2.1.403 - this is defined in file `global.json`, you can change this locally for your version, for CI only for version defined here: [Trident Docs: Generic Build Agent](https://einstein.kcura.com/display/DEVOP/Trident+Docs%3A+Generic+Build+Agent)


## Build Tasks

This repository builds with Powershell through the `.\build.ps1` script. 
It supports standard tasks like `.\build.ps1 compile`, `.\build.ps1 test`, `.\build.ps1 functionaltest`, and `.\build.ps1 package`.

For functional tests, point PowerShell to the root of this repository and provide the necessary arguments for the test settings using `.\DevelopmentScripts\New-TestSettings.ps1 <INSERT_ARGUMENTS_HERE>` before running the functionaltest task.


## Test

Run Functional Tests
> .\build.ps1 -- Builds the code.
> .\DevelopmentScripts\New-TestSettings.ps1 p-dv-vm-<currentVm> -- Generates the settings for local test vm.
> .\build.ps1 FunctionalTest -- Uses the generated settings.
> .\build.ps1 -TaskList Compile, Package, FunctionalTest -RAPVersion 1.0.0.xxx -- Generates RAP with provided versions 


## Online Documentation RAP CD

For more information on RAP CD, [view the documentation in Einstein](https://einstein.kcura.com/x/hRkFCQ)

