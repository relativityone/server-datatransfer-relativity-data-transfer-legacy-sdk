using System;
using System.Data;
using System.Threading.Tasks;
using DataTransfer.Legacy.MassImport.NUnit.Integration.Helpers;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.MassImport.Api;
using Relativity.MassImport.DTO;
using MassImportManager = Relativity.Core.Service.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
    public class ObjectImportTest : MassImportTestBase
    {
        private const string ObjectTypeName = "Orders";
        private const string IdentifierFieldName = "Order"; // test for REL-596735 - mass import fails when identifier field name is 'Order' and multi object field is mapped
        private const string SelfReferenceFieldName = "SelfReference";

        private int _ordersArtifactTypeId;
        private MassImportField _ordersIdentifierField;
        private MassImportField _ordersSelfReferenceField;

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            // create 'Orders' RDO
            _ordersArtifactTypeId = await RdoHelper.CreateObjectTypeAsync(TestParameters, TestWorkspace, ObjectTypeName).ConfigureAwait(false); ;
            var ordersIdentifierField = await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace, _ordersArtifactTypeId).ConfigureAwait(false); ;
            _ordersIdentifierField = await FieldHelper.RenameIdentifierField(
                TestParameters,
                TestWorkspace,
                _ordersArtifactTypeId,
                ordersIdentifierField.ArtifactID,
                newName: IdentifierFieldName).ConfigureAwait(false); ;
            _ordersSelfReferenceField = await FieldHelper.CreateMultiObjectField(
                TestParameters,
                TestWorkspace,
                fieldName: SelfReferenceFieldName,
                destinationRdoArtifactTypeId: _ordersArtifactTypeId,
                associativeRdoArtifactTypeId: _ordersArtifactTypeId).ConfigureAwait(false); ;
        }

        [TearDown]
        public Task TearDown()
        {
            return RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, _ordersArtifactTypeId);
        }

        [Test]
        public async Task ShouldImportObjectsWithOrderIdentifierAndMultiObjectSelfReference() // test for REL-596735 - mass import fails when identifier field name is 'Order' and multi object field is mapped
        {
            // Arrange
            const int expectedArtifactsCreated = 1;
            const int expectedArtifactsUpdated = 0;
            const bool inRepository = true;
            
            ObjectLoadInfo nativeLoadInfo = await this.CreateSampleObjectLoadInfoAsync().ConfigureAwait(false);
			MassImportManager massImportManager = new MassImportManager(false, HelperMock.Object);

            // Act
            MassImportManagerBase.MassImportResults result = massImportManager.RunObjectImport(this.CoreContext, nativeLoadInfo, inRepository);

            // Assert
            this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
            
            DataTable expectedValues = new DataTable(ObjectTypeName);
            expectedValues.Columns.AddRange(new []
            {
                new DataColumn(IdentifierFieldName),
            });

            expectedValues.Rows.Add("ORDER_1");
            expectedValues.Rows.Add("ORDER_2");
            Validator.ThenTheFieldsHaveCorrectValues(this.TestWorkspace, expectedValues, _ordersIdentifierField.DisplayName);
        }

        private async Task<ObjectLoadInfo> CreateSampleObjectLoadInfoAsync()
        {
            string fieldDelimiter = "þþKþþ";
            Relativity.FieldInfo[] fields =
            {
                new Relativity.FieldInfo
                {
                    ArtifactID = _ordersIdentifierField.ArtifactID,
                    Category = Relativity.FieldCategory.Identifier,
                    CodeTypeID = 0,
                    DisplayName = _ordersIdentifierField.DisplayName,
                    EnableDataGrid = false,
                    FormatString = null,
                    ImportBehavior = null,
                    IsUnicodeEnabled = true,
                    TextLength = 255,
                    Type = Relativity.FieldTypeHelper.FieldType.Varchar,
                },
                new Relativity.FieldInfo
                {
                    ArtifactID = _ordersSelfReferenceField.ArtifactID,
                    DisplayName = _ordersSelfReferenceField.DisplayName,
                    Type = Relativity.FieldTypeHelper.FieldType.Objects,
                }
            };

            // build metadata file content
            DataTable fieldValues = new DataTable();
            DataColumn[] columns =
            {
                new DataColumn(WellKnownFields.ControlNumber)
            };

            fieldValues.Columns.AddRange(columns);

            object[] values = new[] { "ORDER_1" };
            fieldValues.Rows.Add(values);

            string dataFileContent = GetMetadata(fieldDelimiter, fieldValues, folders: null, includeMetadataFileId: false);

            // build object file content

            var objectsFile = new[] { new ObjectHelper.ObjectStagingTableRow
                {
                    PrimaryObjectName = "ORDER_1",
                    ObjectTypeID = _ordersArtifactTypeId,
                    FieldID = _ordersSelfReferenceField.ArtifactID,
                    SecondaryObjectName = "ORDER_2"
                }
            };
            string objectsFileContent = ObjectHelper.GetObjectsFile(fieldDelimiter, objectsFile);

            return new ObjectLoadInfo
            {
                ArtifactTypeID = _ordersArtifactTypeId,
                AuditLevel = ImportAuditLevel.FullAudit,
                Billable = true,
                BulkLoadFileFieldDelimiter = fieldDelimiter,
                CodeFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
                DataFileName = await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, dataFileContent).ConfigureAwait(false),
                DataGridFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
                DisableUserSecurityCheck = false,
                ExecutionSource = ExecutionSource.ImportAPI,
                KeyFieldArtifactID = _ordersIdentifierField.ArtifactID,
                LoadImportedFullTextFromServer = false,
                MappedFields = fields,
                MoveDocumentsInAppendOverlayMode = false,
                ObjectFileName = await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, objectsFileContent).ConfigureAwait(false),
                OnBehalfOfUserToken = null,
                Overlay = OverwriteType.Append,
                OverlayArtifactID = -1,
                OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
                Range = null,
                Repository = this.TestWorkspace.DefaultFileRepository,
                RootFolderID = 1003697,
                RunID = Guid.NewGuid().ToString().Replace('-', '_'),
                UploadFiles = false,
                UseBulkDataImport = true,
            };
        }
    }
}