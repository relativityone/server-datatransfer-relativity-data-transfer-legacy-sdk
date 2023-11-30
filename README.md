# DataTransfer Legacy SDK

This is a port of old WebAPI Import API to Kepler API



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

## History

The server-release-12.3-2023 branch was migrated from https://git.kcura.com/projects/DTX/repos/relativity-data-transfer-legacy-sdk/browse?at=refs%2Fheads%2Fserver-release-12.3-2023 by the Server Data Transfer Team. Tag: 23000.19.1-server Branch: server-release-12.3-2023

## Maintainers
This repository is owned by the Server Data Transfer Team. #help-server-data-transfer