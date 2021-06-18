
# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

## [0.1.10]

- Add interceptor for handling unhandled errors

## [0.1.9]

- Fix the Castle dependency issue for IAPICommunicationModeService

- Changed return type of the 'IProductionService.RetrieveBatesByProductionAndDocumentAsync' from 'object()()' to 'ExportDataWrapper'

## [0.1.8]

- Implement permission checking with interceptor

## [0.1.7]

- Use admin workspace context in GetAllDocumentFolderPathsForCaseAsync

## [0.1.6]

- Use correct service context in FileIOService.RemoveTempFileAsync

## [0.1.5]

- Align metrics in entire service with IAPI interceptor approach

## [0.1.4]

- Align logging in entire service with IAPI interceptor approach

## [0.1.3]

- Add connection mode endpoint

## [0.1.1]

- Distribute SDK as nuget

## [0.1.0]

- Initial work
