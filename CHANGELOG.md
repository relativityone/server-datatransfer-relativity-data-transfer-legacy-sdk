
# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

## [0.1.22]
- REL-573917 Handle exception when IAPICommunicationMode is not set

## [0.1.21]

- REL-574894 Remove FreeImageNET.dll from application files

- REL-574894 Update Relativity.DataExchange.Client.SDK to 1.14.16, version without FreeImageNET.dll

## [0.1.20]

- REL-572781 Update CI Test Feature Attributes

## [0.1.19]

- Update Setup for functional tests

- Add required changes to runtest for Ring 0 CI CD

## [0.1.18]

- Handle exceptions inside interceptors

## [0.1.17]

- Throw original exception which inherits from ServiceException

## [0.1.16]

- Include original exception type and message in ServiceException message

## [0.1.15]

- Include original exception message in ServiceException message

- Fix null reference exception when creating DataSetWrapper from null result

## [0.1.14]

- Add Relativity logo to SDK nuget to meet publish to nuget.org requirements

## [0.1.13]

- Add missing ut for services

## [0.1.12]

- Hash sensitive values in logs

## [0.1.11]

- Add interceptor for communication mode toggle checking

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
