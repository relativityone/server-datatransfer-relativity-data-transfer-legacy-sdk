# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

## [0.4.0] - 05-18-2023

## [0.2.11]
- REL-576997 Catch exception when folder does not exist and use folder ArtifactID instead of folder name

## [0.2.10]
- REL-576995 Do not throw the exception when RunId is empty - there is no temp table in DB

### Added

- NuGet 6 centralized package management support

### Changed

- All platform dependencies align with the monolith
- RTF 10.1.0 to support running the functional tests against a TestVM
- Trident CI pipeline deploys the Server 2022 image and updates to the latest Server 2023


## [0.3.0] - 05-17-2023

### Added

- Added a new changelog.md
- Added CODEOWNERS

### Changed

- Archived the existing changelog.md file
- Updated README.md and Tridentfile