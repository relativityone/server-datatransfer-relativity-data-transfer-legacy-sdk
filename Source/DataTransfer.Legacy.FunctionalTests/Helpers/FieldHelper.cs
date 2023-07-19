using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;
using Polly;
using Relativity.Services.ChoiceManager;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Models;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	internal static class FieldHelper
	{
		public static async Task<SDK.ImportExport.V1.Models.FieldInfo> ReadIdentifierField(int workspaceId)
		{
			var fieldsToRead = new[] { FieldFieldNames.Name, FieldFieldNames.Unicode, FieldFieldNames.Length };
			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = (int)ArtifactType.Field,
				},
				Condition = $"'{FieldFieldNames.IsIdentifier}' == True",
				Fields = fieldsToRead.Select(fieldName => new FieldRef { Name = fieldName }).ToArray(),
			};

			QueryResult queryResult;
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (var objectManager = serviceFactory.GetServiceProxy<IObjectManager>())
			{
				queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);
			}

			RelativityObject field = queryResult.Objects.Single();
			return new SDK.ImportExport.V1.Models.FieldInfo
			{
				ArtifactID = field.ArtifactID,
				DisplayName = (string)field["Name"].Value,
				Type = SDK.ImportExport.V1.Models.FieldType.Varchar,
				Category = SDK.ImportExport.V1.Models.FieldCategory.Identifier,
				IsUnicodeEnabled = (bool)field["Unicode"].Value,
				TextLength = (int)field["Length"].Value,
				EnableDataGrid = false,
				FormatString = null,
				ImportBehavior = null,
				CodeTypeID = 0
			};
		}



		private static async Task<int> GetCodeTypeIdForFieldNameAsync(
			IKeplerServiceFactory serviceFactory,
			int workspaceId,
			string fieldName)
		{
			using (var choiceManager = serviceFactory.GetServiceProxy<IChoiceManager>())
			{
				return await choiceManager
					.GetCodeTypeIdByNameAsync(workspaceId, fieldName)
					.ConfigureAwait(false);
			}
		}

		public static Task<int> CreateLongTextFieldAsync(string fieldName, int destinationRdoArtifactTypeId, bool isOpenToAssociations, int workspaceId)
		{
			var request = new LongTextFieldRequest
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = destinationRdoArtifactTypeId },
				OpenToAssociations = isOpenToAssociations,
				HasUnicode = true,
			};

			return CreateFieldAsync(request, workspaceId);
		}

		public static Task<int> CreateFixedLengthTextFieldAsync(string fieldName, int objectArtifactTypeId, bool isOpenToAssociations, int length, int workspaceId)
		{
			var request = new FixedLengthFieldRequest()
			{
				Name = fieldName,
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = objectArtifactTypeId },
				OpenToAssociations = isOpenToAssociations,
				HasUnicode = false,
				Length = length,
				Wrapping = false,
				Width = 50,
				IsRequired = false,
				AllowHtml = false,
				AllowGroupBy = false,
				AllowSortTally = false,
				AllowPivot = false,
			};

			return FieldHelper.CreateFieldAsync(request, workspaceId);
		}


		private static Task<int> CreateFieldAsync(BaseFieldRequest fieldRequest, int workspaceId)
		{
			var supportedFieldTypesToCreateMethodMapping = new Dictionary<Type, Func<IFieldManager, Task<int>>>
			{
				[typeof(SingleObjectFieldRequest)] = (manager) => manager.CreateSingleObjectFieldAsync(workspaceId, fieldRequest as SingleObjectFieldRequest),
				[typeof(MultipleObjectFieldRequest)] = (manager) => manager.CreateMultipleObjectFieldAsync(workspaceId, fieldRequest as MultipleObjectFieldRequest),
				[typeof(SingleChoiceFieldRequest)] = (manager) => manager.CreateSingleChoiceFieldAsync(workspaceId, fieldRequest as SingleChoiceFieldRequest),
				[typeof(MultipleChoiceFieldRequest)] = (manager) => manager.CreateMultipleChoiceFieldAsync(workspaceId, fieldRequest as MultipleChoiceFieldRequest),
				[typeof(WholeNumberFieldRequest)] = (manager) => manager.CreateWholeNumberFieldAsync(workspaceId, fieldRequest as WholeNumberFieldRequest),
				[typeof(LongTextFieldRequest)] = (manager) => manager.CreateLongTextFieldAsync(workspaceId, fieldRequest as LongTextFieldRequest),
				[typeof(FixedLengthFieldRequest)] = (manager) => manager.CreateFixedLengthFieldAsync(workspaceId, fieldRequest as FixedLengthFieldRequest),
				[typeof(FileFieldRequest)] = (manager) => manager.CreateFileFieldAsync(workspaceId, fieldRequest as FileFieldRequest),
				[typeof(DateFieldRequest)] = (manager) => manager.CreateDateFieldAsync(workspaceId, fieldRequest as DateFieldRequest),
				[typeof(DecimalFieldRequest)] = (manager) => manager.CreateDecimalFieldAsync(workspaceId, fieldRequest as DecimalFieldRequest),
			};

			return ExecuteMethodForFieldType(supportedFieldTypesToCreateMethodMapping, fieldRequest);
		}

		private static Task<int> ExecuteMethodForFieldType(Dictionary<Type, Func<IFieldManager, Task<int>>> methodMapping, BaseFieldRequest fieldRequest)
		{
			Func<IFieldManager, Task<int>> method = GetMethodForFieldType(methodMapping, fieldRequest);
			return ExecuteMethod(method);
		}

		private static Func<IFieldManager, Task<int>> GetMethodForFieldType(Dictionary<Type, Func<IFieldManager, Task<int>>> methodMapping, BaseFieldRequest fieldRequest)
		{
			if (!methodMapping.TryGetValue(fieldRequest.GetType(), out var method))
			{
				throw new InvalidOperationException(
					"This method does not support requested field type. Please see source code of that method for more details.");
			}

			return method;
		}

		private static async Task<int> ExecuteMethod(Func<IFieldManager, Task<int>> method)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (var fieldManager = serviceFactory.GetServiceProxy<IFieldManager>())
			{
				int fieldId = await Policy.Handle<Exception>()
					.WaitAndRetryAsync(3, retryNumber => TimeSpan.FromSeconds(3 ^ retryNumber))
					.ExecuteAsync(() => method(fieldManager)).ConfigureAwait(false);
				return fieldId;
			}
		}

		private static async Task<int> CreateOrUpdateFieldAsync(
			int workspaceId,
			BaseFieldRequest fieldRequest)
		{
			var results = await QueryFieldByNameAsync(
				workspaceId,
				fieldRequest.Name).ConfigureAwait(false);

			if (results.TotalCount == 0)
			{
				return await CreateFieldAsync(fieldRequest, workspaceId)
					.ConfigureAwait(false);
			}

			return await UpdateFieldAsync(workspaceId, results.Objects[0].ArtifactID, fieldRequest)
				.ConfigureAwait(false);
		}

		private static Task<int> UpdateFieldAsync(int workspaceId, int fieldIdentifier, BaseFieldRequest fieldRequest)
		{
			var supportedFieldTypesToCreateMethodMapping = new Dictionary<Type, Func<IFieldManager, Task>>
			{
				[typeof(SingleObjectFieldRequest)] = (manager) => manager.UpdateSingleObjectFieldAsync(workspaceId, fieldIdentifier, fieldRequest as SingleObjectFieldRequest),
				[typeof(MultipleObjectFieldRequest)] = (manager) => manager.UpdateMultipleObjectFieldAsync(workspaceId, fieldIdentifier, fieldRequest as MultipleObjectFieldRequest),
				[typeof(SingleChoiceFieldRequest)] = (manager) => manager.UpdateSingleChoiceFieldAsync(workspaceId, fieldIdentifier, fieldRequest as SingleChoiceFieldRequest),
				[typeof(MultipleChoiceFieldRequest)] = (manager) => manager.UpdateMultipleChoiceFieldAsync(workspaceId, fieldIdentifier, fieldRequest as MultipleChoiceFieldRequest),
				[typeof(WholeNumberFieldRequest)] = (manager) => manager.UpdateWholeNumberFieldAsync(workspaceId, fieldIdentifier, fieldRequest as WholeNumberFieldRequest),
				[typeof(LongTextFieldRequest)] = (manager) => manager.UpdateLongTextFieldAsync(workspaceId, fieldIdentifier, fieldRequest as LongTextFieldRequest),
				[typeof(FixedLengthFieldRequest)] = (manager) => manager.UpdateFixedLengthFieldAsync(workspaceId, fieldIdentifier, fieldRequest as FixedLengthFieldRequest),
				[typeof(FileFieldRequest)] = (manager) => manager.UpdateFileFieldAsync(workspaceId, fieldIdentifier, fieldRequest as FileFieldRequest),
			};

			Func<IFieldManager, Task<int>> AddReturnValue(Func<IFieldManager, Task> f) => manager => f.Invoke(manager).ContinueWith((task) => fieldIdentifier);
			Dictionary<Type, Func<IFieldManager, Task<int>>> methodsWithReturnValues =
				supportedFieldTypesToCreateMethodMapping.ToDictionary(x => x.Key, x => AddReturnValue(x.Value));

			return ExecuteMethodForFieldType(methodsWithReturnValues, fieldRequest);
		}

		private static async Task<QueryResult> QueryFieldByNameAsync(int workspaceId, string fieldName)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (var objectManager = serviceFactory.GetServiceProxy<IObjectManager>())
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.Field,
					},
					Condition = $"'Name' == '{fieldName}'",
				};
				QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 0, 1).ConfigureAwait(false);
				return queryResult;
			}
		}

		public static async Task DeleteFieldAsync(
			int workspaceId,
			int fieldId)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (var fieldManager = serviceFactory.GetServiceProxy<IFieldManager>())
			{
				await fieldManager.DeleteAsync(workspaceId, fieldId).ConfigureAwait(false);
			}
		}
	}
}
