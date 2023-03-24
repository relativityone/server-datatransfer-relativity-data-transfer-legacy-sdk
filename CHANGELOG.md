# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),

## [0.3.21]
- REL-797147 - reduce chunk size when lenght of serialized data is too large to be deserialized

## [0.3.20]
- REL-829772 - Send event with number of empty texts passed to DataGrid

## [0.3.19]
- REL-827198 Revert changes

## [0.3.18]
- REL-827198 Updated Relativity.DataGrid to version 17.2.36. Flags processing files as linked text.

## [0.3.17]
- DEVOPS-183123 - Remove LinkDataGridRecords logic from code

## [0.3.16]
- REL-748334 - Remove DisableBatchResultCacheToggle

## [0.3.15]
- REL-635344 Send logs to NR

## [0.3.14]
- REL-826790 - Read File Repository info from existing Kepler endpinds

## [0.3.13]
- REL-824414 - More logging in case of FormatException

## [0.3.12]
- REL-635344 Remove redundant activity status ok

## [0.3.10]
- REL-635344 Set trace status based on error

## [0.3.9]
- REL-635344 Integrate legacy traces with CAL

## [0.3.8]
- REL-824414 - Handle FormatException in GetErrors endpoinds

## [0.3.7]
- REL-823658 - Update DataGrid to version 17.3.23. Fix export performance.

## [0.3.6]
- REL-823658 - Update DataGrid to version 17.3.22. Initialize datagrid with CAL reference.

## [0.3.5]
- REL-812680 - Limit the usage of the new DataGridContext constructor to linking ExtractedText files workflow

## [0.3.4]
- REL-812680 - Limit usage of Data Grid constructor, add metrics

## [0.3.3]
- REL-812680 - Update DataGrid to version 17.3.17

## [0.3.2]
- REL-818462 - Remove all limit document count logic

## [0.3.1]
- REL-812680 - Update DataGrid version, use toggle for DataGrid

## [0.3.0]
- REL-812680 - Update DataGrid to version supporting CAL

## [0.2.72]
- REL-635344 - Distributed Tracing instance id

## [0.2.71]
- REL-816343 - Block exporting productions with redacted natives

## [0.2.70]
- REL-635344 - OpenTelemetry and Distributed Tracing

## [0.2.69]
- REL-718348 - Catch and log for all Exception types during folder creation

## [0.2.68]
- REL-812680 - Force DG version and update core

## [0.2.67]
- REL-804941 - Add object and field information to ErrorDuplicateAssociatedObject message

## [0.2.66]
- REL-622708 - Add support for import PDF files in production

## [0.2.65]
- REL-770037 - Add releye telemetry events

## [0.2.64]
- REL-790064 - Add missing CAL library

## [0.2.63]
- REL-718348 - Map InsufficientAccessControlListPermissions into Kepler PermissionDeniedException

## [0.2.62]
- REL-718348 - Add logging in case of KeyNotFoundException in folder creation with functional tests

## [0.2.61]
- REL-797246 - Use CAL to read Extracted Text files

## [0.2.60]
- REL-718348 - Revert provious changes

## [0.2.59]
- REL-718348 - Add logging in case of KeyNotFoundException in folder creation

## [0.2.58]
- REL-795902 - Add metrics with number of retires executed for bulk insert

## [0.2.57]
- REL-790064 - Update DataGrid to version 17.3.1 with CAL integration

## [0.2.56]
- REL-789615 - Change MassResult serialization and order of temp tables cleanup

## [0.2.55]
- REL-789615 - Change exception type to ConflictException with message

## [0.2.54]
- REL-789615 - Add cache with result for all BulkImport methods - allows to retry this methods

## [0.2.53]
- REL-769990 - Rethrow WorkspaceStatusException as NotFoundException in PermissionCheckInterceptor

## [0.2.52]
- REL-769990 - Add support for non super admin users

## [0.2.51]
- REL-772566 - Add r1 team id for RelEye for MassImport part

## [0.2.50]
- REL-765588 - Add r1 team id for RelEye

## [0.2.49]
- REL-766933 - Fix sending Count APM metrics to New Relic

## [0.2.48]
- REL-765639 - Add workspace creation for compatibility tests

## [0.2.47]
- REL-754154 - Insert ancestors query optimization

## [0.2.46]
- REL-759042 - Fix failing functional tests

## [0.2.45]
- REL-759042 - Fix extracted text image import on a broken workspace

## [0.2.44]
- REL-743070 - Add setting hasImages to 'No' in production import

## [0.2.43]
- REL-748487 - Put the default value of MassImportSqlTimeout setting into MassImport code

## [0.2.42]
- REL-710543 - Add support for pdf files in images and production import

## [0.2.41]
- REL-596735 - Fixed issue with importing RDOs which uses reserved SQL keywords as an identifier field name

## [0.2.40]
- REL-730175 - Catch Core.Exception.Permission and rethrow as known PermissionDeniedException

## [0.2.39]
- REL-697865 - restore metrics concerning files created count and documnets created count(BulkImportService -> LogTelemetryMetricsForImport) 

## [0.2.38]
- REL-714384  Rethrow BaseException as NotFoundException in UnhandledExceptionInterceptor

## [0.2.37]
- REL-717154 - MassImport nuget for OM, correct dependencies version

## [0.2.36]
- REL-717154 - MassImport nuget for OM

## [0.2.35]
- REL-682882 - Improve delete natives query

## [0.2.34]
- DEVOPS-159751 - changed metrics logging level from Warning to Information
- DEVOPS-159751 - send field details metrics once per import job instead of sending it for each batch

## [0.2.33]
- REL-671246 - Add retries for any error in AppLock

## [0.2.32]
- REL-686998 - Remove usage of IFileSystemManager from tests

## [0.2.31]
- REL-691656 - Improve error message for bad associative object type error

## [0.2.30]
- REL-691656 - Copy ImportStatus enum to DataTransfer.Legacy

## [0.2.29]
- REL-683467 - Add more retires for NoBcpDirectoryException

## [0.2.28]
- REL-669697 - Catch Relativity.Core.Exception.Permission and throw as PermissionDeniedException to stop automatic retries in Import API

## [0.2.27]
- REL-680339 - Add retires for NoBcpDirectoryException

## [0.2.26]
- REL-677115 - Use Polly as retry policy and log exceptions as warning

## [0.2.25]
- REL-665898 - Splunk queries field statistics

## [0.2.24]
- REL-665917 - Remove temp DataGrid files when there is no datagrid data to import in images

## [0.2.23]
- REL-666446 - Change log level to Warning and add more info about retries values in logs

## [0.2.22]
- REL-664388 - Respect preview user in MassImport

## [0.2.21]
- REL-658054 - added RetryPolicyFactory.CreateDeadlockExceptionAndResultRetryPolicy which handles deadlock exceptions returned as error message from function. Used that policy in CodeService.

## [0.2.20]
- REL-658054 - RetryPolicyFactory.CreateDeadlockRetryPolicy checks if any exception in a chain was caused by deadlock.

## [0.2.19]
- REL-662147 - used property from cloned NativeLoadInfo and ImageLoadInfo DTOs instead of the passed value as property through the context.

## [0.2.18]
- REL-661511 - cloned ImageLoadInfo dto from the relativity core.

## [0.2.17]
- REL-661435 - cloned NativeLoadInfo and ObjectLoadInfo dtos from the relativity core.

## [0.2.16]
- REL-658092 - added BulkFileSharePath property for images API and overwrite BCP path with value given in this new property.

## [0.2.15]
- REL-658092 - added BulkFileSharePath property for API and overwrite BCP path with value given in this new property.

## [0.2.14]
- REL-658054 - added retries for deadlock exception in CodeService.CreateEncodedAsync

## [0.2.13]
- REL-651044 - Remove MassImportImprovementsToggle

## [0.2.12]
- REL-637797 Remove CreateItemErrorWhenIdentifierIsNullToggle and UseNewChoicesQueryToggle

## [0.2.11]
- REL-576997 Catch exception when folder does not exist and use folder ArtifactID instead of folder name

## [0.2.10]
- REL-576995 Do not throw the exception when RunId is empty - there is no temp table in DB

## [0.2.9]
- REL-593286 Update RTF dependencies to increase timeout of CI tests

## [0.2.8]
- REL-593286 Change index on temp tables to allow import of records with length of 450

## [0.2.7]
- REL-644482 Fix Assert condition in CI Test #2

## [0.2.6]
- REL-644482 Fix Assert condition in CI Test

## [0.2.5]
- REL-644482 Fix Importing datagrid records in mass import

## [0.2.4]
- REL-641071 RDO duplicates data loss fix

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
