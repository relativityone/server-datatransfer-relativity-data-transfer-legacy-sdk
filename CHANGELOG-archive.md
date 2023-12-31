
# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

## [0.2.4]
- REL-825272 Code Isolation - relativity-data-transfer-legacy-sdk

## [0.2.3]
- REL-630923 Change default IAPICommunicationMode to ForceKepler

## [0.2.2]
- REL-618414 add retries for exception "Bad or inaccessible location specified in external data source"

## [0.2.1]
- REL-619788 bump version to rerun the pipeline

## [0.2.0]
- REL-619788 Decouple MassImport from relativity-core 

## [0.1.39]
- REL-593815 Update RTF, RTF API, RingSetup

## [0.1.38]
- REL-600343 Reoreder interceptors to catch all unhandled exception, improve logging

## [0.1.37]
- REL-596365 New endpoint for TAPI configuration

## [0.1.36]
- REL-595509 Structured and enhanced metrics

## [0.1.35]
- REL-591402 add metrics to APM (arguments)

## [0.1.34]
- REL-591402 add metrics to APM (TargetType, Method, ElapsedMilliseconds)

## [0.1.33]
- REL-591402 publish metrics to APM

## [0.1.32]
- REL-581451 set `disableSignedModule` to true in Castle's `DefaultProxyFactory`

## [0.1.31]
- REL-587119 - Added Castle dll's to RAP

## [0.1.30]
- REL-574905 ManualDeploy Job for Trident created

## [0.1.29]
- REL-574902 Performacne Test

## [0.1.28]
- REL-577456 Added DataTransfer.Legacy.PostInstallEventHandler for upserting instance setting

## [0.1.27]
- REL-581451 Updated Relativity.Kepler to version 2.15.3 (version used in the Relativity Prairie Smoke)

## [0.1.26]
- REL-577406 Change the type of exception, NotFoundException is expected by the ImportApi

## [0.1.25]
- REL-574903 New Healt Check endpoint

## [0.1.24]
- REL-572781 Disable R# errors for usage of obsolete methods in tests

## [0.1.23]
- REL-577929 Fix namespace to correctly analyze unit test coverage

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
