using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Core.DTO;

namespace Relativity.MassImport.Api
{
	/// <summary>
	/// Helper methods preparing data to new mass import interface format
	/// </summary>
	public static class MassImportHelper
	{
		public static IArtifact SyncArtifactFieldsWithTemplateFields(IArtifact templateArtifact, IArtifact currentArtifact)
		{
			// Validate that fields exist in template
			foreach (Field field in currentArtifact.Fields)
			{
				Field templateArtifactField = FindMatchingField(field, templateArtifact.Fields);
				if (templateArtifactField == null)
				{
					var identifier = currentArtifact.Fields.SingleOrDefault(f => f.FieldCategory == FieldCategory.Identifier)?.Value;
					throw new MassImportException($"Field with Name {field.DisplayName} and ArtifactID {field.ArtifactID} exists in the artifact with identifier {identifier} but not the template artifact");
				}
			}

			// Ensure that the artifact being created has the same fields as the template in the same order
			List<Field> fieldList = new List<Field>();
			foreach (Field templateField in templateArtifact.Fields)
			{
				Field matchingField = FindMatchingField(templateField, currentArtifact.Fields);
				if (matchingField == null)
				{
					fieldList.Add(templateField);
				}
				else
				{
					fieldList.Add(matchingField);
				}
			}
			currentArtifact.Fields = fieldList.ToArray();
			
			return currentArtifact;
		}

		private static Field FindMatchingField(Field field, IEnumerable<Field> fields)
		{
			foreach (Relativity.Core.DTO.Field listField in fields)
			{
				if (listField.ArtifactID > 0 && listField.ArtifactID == field.ArtifactID)
				{
					return listField;
				}
					
				if (!string.IsNullOrEmpty(listField.DisplayName) & !string.IsNullOrEmpty(field.DisplayName))
				{
					if (string.Compare(listField.DisplayName, field.DisplayName, true) == 0)
					{
						return listField;
					}
				}
			}
			return null/* TODO Change to default(_) if this is not a reference type */;
		}

		public static MassImportField CreateFieldInfoFromDTO(Field fieldDTO)
		{
			MassImportField newFieldInfo = new MassImportField();
			newFieldInfo.ArtifactID = fieldDTO.ArtifactID;
			newFieldInfo.Category = fieldDTO.FieldCategory;
			newFieldInfo.DisplayName = fieldDTO.DisplayName;
			newFieldInfo.Type = fieldDTO.FieldType;
			newFieldInfo.IsUnicodeEnabled = fieldDTO.UseUnicodeEncoding;
			newFieldInfo.ImportBehavior = fieldDTO.ImportBehavior;
			newFieldInfo.EnableDataGrid = fieldDTO.EnableDataGrid;
			newFieldInfo.CodeTypeID = fieldDTO.CodeTypeID.GetValueOrDefault();
			newFieldInfo.AssociativeArtifactTypeID = fieldDTO.AssociativeArtifactTypeID.GetValueOrDefault();
			newFieldInfo.FormatString = fieldDTO.FormatString ?? "";
			newFieldInfo.TextLength = fieldDTO.MaxLength > 0 ? fieldDTO.MaxLength.GetValueOrDefault() : 0;
		
			return newFieldInfo;
		}

		public static void SetObjectFieldContainsArtifactIdFlag(this IEnumerable<MassImportField> fields)
		{
			foreach (MassImportField field in fields)
			{
				if (field.Type == FieldTypeHelper.FieldType.Object)
				{
					field.ImportBehavior = FieldInfo.ImportBehaviorChoice.ObjectFieldContainsArtifactId;
				}
			}			
		}

		public static MassImportArtifact CreateMassImportArtifact(this IArtifact artifact)
		{
			return new MassImportArtifact(
				fieldValues: artifact.Fields.Select(field => field.Value).ToList(), 
				parentFolderId: artifact.ParentArtifactID.GetValueOrDefault());
		}

		//source: https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Batch.cs
		//TODO: move to IEnumerableExtensions
		internal static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
		{
			T[] bucket = null;
			var count = 0;

			foreach (var item in source)
			{
				if (bucket == null)
					bucket = new T[size];

				bucket[count++] = item;

				if (count != size)
					continue;

				yield return bucket.Select(x => x);

				bucket = null;
				count = 0;
			}

			// Return the last bucket with all remaining elements
			if (bucket != null && count > 0)
			{
				Array.Resize(ref bucket, count);
				yield return bucket;
			}
		}
	}
}