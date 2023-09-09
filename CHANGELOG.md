# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [23000.19.1] - 09-08-2023

### Changed

- Bumped the application version to align with the Server 2023 application versioning strategy ADR.

## [23000.19.0] - 09-04-2023

### Changed

- Bumped the application version to align with the Server 2023 application versioning strategy ADR.

## [0.19.0] - 08-17-2023
 
### Changed
 
- [REL-868461](https://jira.kcura.com/browse/REL-868461) - Create release branch for RAPCD
- Official Relativity 2023 12.3 release.
- The SUT configuration upgrades the previous release image to the latest release image.

## [0.18.0]   - 08-17-2023

### Changed
- Cumulative merge of all backported changes

## [0.17.0]   - 08-07-2023

 ## Changed

- [REL-857961](https://jira.kcura.com/browse/REL-857961) -  Backport relativity-data-transfer-legacy-sdk - Backported from [REL-795902](https://jira.kcura.com/browse/REL-795902), [REL-789615](https://jira.kcura.com/browse/REL-789615) and [REL-718348](https://jira.kcura.com/browse/REL-718348) ticket


## [0.16.0]   - 08-07-2023

 ## Changed

- [REL-857954](https://jira.kcura.com/browse/REL-857954) -  Backport relativity-data-transfer-legacy-sdk - Backported from [REL-765639](https://jira.kcura.com/browse/REL-765639), [REL-766933](https://jira.kcura.com/browse/REL-766933) and [REL-728085](https://jira.kcura.com/browse/REL-728085) ticket


## [0.15.0] - 08-03-2023

### Changed

- [REL-857947](https://jira.kcura.com/browse/REL-857947) - Backport relativity-data-transfer-legacy-sdk - Backported from [REL-743070](https://jira.kcura.com/browse/REL-743070),
 [REL-759042](https://jira.kcura.com/browse/REL-759042), [REL-754154](https://jira.kcura.com/browse/REL-754154) ticket

## [0.14.0] - 08-02-2023

### Changed

- [REL-857940](https://jira.kcura.com/browse/REL-857940) - Backport relativity-data-transfer-legacy-sdk - Backported from [REL-714384](https://jira.kcura.com/browse/REL-714384),
 [REL-730175](https://jira.kcura.com/browse/REL-730175), [REL-596735](https://jira.kcura.com/browse/REL-596735) ticket from Server 2022 release

## [0.13.0]   - 07-31-2023

 ## Changed

- [REL-857934](https://jira.kcura.com/browse/REL-857934) -  Backport relativity-data-transfer-legacy-sdk - Backported from [DEVOPS-159751](https://jira.kcura.com/browse/DEVOPS-159751), [REL-682882](https://jira.kcura.com/browse/REL-682882) and [REL-697865](https://jira.kcura.com/browse/REL-697865) ticket

## [0.12.0]   - 07-31-2023

## Changed

- [REL-857924](https://jira.kcura.com/browse/REL-857924) -  Backport relativity-data-transfer-legacy-sdk - Backported from [REL-691656](https://jira.kcura.com/browse/REL-691656) and [REL-671246](https://jira.kcura.com/browse/REL-671246) ticket 

## [0.11.0] - 07-31-2023

### Changed

- [REL-857917](https://jira.kcura.com/browse/REL-857917) - relativity-data-transfer-legacy-sdk - Backported from [REL-669697](https://jira.kcura.com/browse/REL-669697) and [REL-683467](https://jira.kcura.com/browse/REL-683467)

## [0.10.0] - 07-20-2023

### Changed

- [REL-857896](https://jira.kcura.com/browse/REL-857896) - Backport relativity-data-transfer-legacy-sdk - Backported from [REL-658092](https://jira.kcura.com/browse/REL-658092) ticket

## [0.9.0] - 07-26-2023

### Changed

- [REL-857911](https://jira.kcura.com/browse/REL-857911) - relativity-data-transfer-legacy-sdk - Backported from [REL-658054](https://jira.kcura.com/browse/REL-658054) and [REL-666446](https://jira.kcura.com/browse/REL-666446), [REL-665917](https://jira.kcura.com/browse/REL-665917) ticket from Server 2022 release

## [0.8.0] - 07-06-2023

### Changed

- [REL-857349](https://jira.kcura.com/browse/REL-857349) - relativity-data-transfer-legacy-sdk - Backported from [REL-637797](https://jira.kcura.com/browse/REL-637797), [REL-651044](https://jira.kcura.com/browse/REL-651044) ticket from Server 2022 release 

## [0.7.0] - 07-18-2023

### Changed

- [REL-857343](https://jira.kcura.com/browse/REL-857343) - Fixing conversion issue from type 'DBNull' to type 'String' - Backported ticket [REL-576997] from server 2022 release

## [0.6.0] - 07-06-2023

### Changed

- [REL-857337](https://jira.kcura.com/browse/REL-857337) - Unrecognized Guid format - Backported [REL-576995](https://jira.kcura.com/browse/REL-576995) ticket from Server 2022 release 

## [0.5.0] - 07-06-2023

### Changed

- [REL-857329](https://jira.kcura.com/browse/REL-857329) - Backport relativity-data-transfer-legacy-sdk - Backported [REL-641071](https://jira.kcura.com/browse/REL-641071),[REL-644482](https://jira.kcura.com/browse/REL-644482),[REL-593286](https://jira.kcura.com/browse/REL-593286) tickets from Server 2022 release 

## [0.4.0] - 05-18-2023

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