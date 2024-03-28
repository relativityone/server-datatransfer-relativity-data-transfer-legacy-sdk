using System.Linq;
using System.Threading.Tasks;
using Relativity;
using Relativity.MassImport.Api;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace MassImport.NUnit.Integration.Helpers
{
	internal static class FieldHelper
	{
		public static Task<MassImportField> ReadIdentifierField(IntegrationTestParameters parameters, TestWorkspace workspace)
			=> ReadIdentifierField(parameters, workspace, (int)ArtifactType.Document);

		public static async Task<MassImportField> ReadIdentifierField(IntegrationTestParameters parameters, TestWorkspace workspace, int artifactTypeId)
		{
			var fieldsToRead = new[] { "Name", "Unicode", "Length" };
			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Field,
				},
				Condition = $"'Object Type Artifact Type ID' IN ['{artifactTypeId}'] AND 'Is Identifier' == True",
				Fields = fieldsToRead.Select(fieldName => new FieldRef { Name = fieldName }).ToArray(),
			};

			QueryResult queryResult;
			using (IObjectManager objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
			{
				queryResult = await objectManager.QueryAsync(workspace.WorkspaceId, queryRequest, 0, 1).ConfigureAwait(false);
			}

			RelativityObject fieldFields = queryResult.Objects.Single();

			return new MassImportField
			{
				ArtifactID = fieldFields.ArtifactID,
				Category = Relativity.FieldCategory.Identifier,
				Type = FieldTypeHelper.FieldType.Varchar,
				DisplayName = (string)fieldFields["Name"].Value,
				IsUnicodeEnabled = (bool)fieldFields["Unicode"].Value,
				TextLength = (int)fieldFields["Length"].Value
			};
		}

		public static async Task<MassImportField> CreateSingleChoiceField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName,
			int destinationRdoArtifactTypeId)
		{
			var request = new SingleChoiceFieldRequest
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				HasUnicode = true,
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateSingleChoiceFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			int codeTypeId = await GetCodeTypeIdForFieldName(parameters, workspace, fieldName).ConfigureAwait(false);

			return new MassImportField
			{
				ArtifactID = fieldId,
				CodeTypeID = codeTypeId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.Code,
			};
		}

		public static async Task<MassImportField> CreateMultiChoiceField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName,
			int destinationRdoArtifactTypeId)
		{
			var request = new MultipleChoiceFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				HasUnicode = true,
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateMultipleChoiceFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			int codeTypeId = await GetCodeTypeIdForFieldName(parameters, workspace, fieldName).ConfigureAwait(false);

			return new MassImportField
			{
				ArtifactID = fieldId,
				CodeTypeID = codeTypeId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.MultiCode,
			};
		}

		public static async Task<MassImportField> CreateFixedLengthTextField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName,
			int destinationRdoArtifactTypeId)
		{
			var request = new FixedLengthFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				Length = 255,
				HasUnicode = true,
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateFixedLengthFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			return new MassImportField
			{
				ArtifactID = fieldId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.Varchar,
				TextLength = request.Length,
			};
		}

		public static async Task<MassImportField> RenameIdentifierField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			int objectTypeArtifactId,
			int fieldId,
			string newName)
		{
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				FieldResponse identifierField = await fieldManager.ReadAsync(workspace.WorkspaceId, fieldId).ConfigureAwait(false);
				var request = new FixedLengthFieldRequest
				{
					Name = newName,
					ObjectType = identifierField.ObjectType,
					Length = identifierField.Length,
				};
				await fieldManager.UpdateFixedLengthFieldAsync(workspace.WorkspaceId, fieldId, request).ConfigureAwait(false);

				return new MassImportField
				{
					ArtifactID = fieldId,
					DisplayName = newName,
					Type = FieldTypeHelper.FieldType.Varchar,
					TextLength = identifierField.Length,
				};
			}
		}

		public static async Task<MassImportField> CreateMultiObjectField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName,
			int destinationRdoArtifactTypeId,
			int associativeRdoArtifactTypeId)
		{
			var request = new MultipleObjectFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				AssociativeObjectType = new ObjectTypeIdentifier { ArtifactTypeID = associativeRdoArtifactTypeId },
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateMultipleObjectFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			return new MassImportField
			{
				ArtifactID = fieldId,
				AssociativeArtifactTypeID = associativeRdoArtifactTypeId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.Objects,
			};
		}

		public static async Task<MassImportField> CreateSingleObjectField(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName,
			int destinationRdoArtifactTypeId,
			int associativeRdoArtifactTypeId)
		{
			var request = new SingleObjectFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				AssociativeObjectType = new ObjectTypeIdentifier { ArtifactTypeID = associativeRdoArtifactTypeId },
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateSingleObjectFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			return new MassImportField
			{
				ArtifactID = fieldId,
				AssociativeArtifactTypeID = associativeRdoArtifactTypeId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.Object,
			};
		}

		public static async Task<FieldInfo> CreateWholeNumberField(IntegrationTestParameters parameters, TestWorkspace workspace, string fieldName, int artifactTypeId)
		{
			var request = new WholeNumberFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = artifactTypeId },
			};

			int fieldId;
			using (var fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				fieldId = await fieldManager.CreateWholeNumberFieldAsync(workspace.WorkspaceId, request).ConfigureAwait(false);
			}

			return new MassImportField
			{
				ArtifactID = fieldId,
				DisplayName = fieldName,
				Type = FieldTypeHelper.FieldType.Integer,
			};
		}

		private static async Task<int> GetCodeTypeIdForFieldName(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string fieldName)
		{
			using (var choiceManager = ServiceHelper.GetServiceProxy<Relativity.Services.ChoiceManager.IChoiceManager>(parameters))
			{
				return await choiceManager
					.GetCodeTypeIdByNameAsync(workspace.WorkspaceId, fieldName)
					.ConfigureAwait(false);
			}
		}

		public static async Task DeleteFieldAsync(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			int fieldId)
		{
			using (IFieldManager fieldManager = ServiceHelper.GetServiceProxy<IFieldManager>(parameters))
			{
				await fieldManager.DeleteAsync(workspace.WorkspaceId, fieldId).ConfigureAwait(false);
			}
		}
	}
}