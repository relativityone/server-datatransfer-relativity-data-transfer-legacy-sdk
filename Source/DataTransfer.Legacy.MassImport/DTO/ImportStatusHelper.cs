using System;
using System.Collections.Generic;

namespace Relativity.MassImport.DTO
{
	[Flags()]
	public enum ImportStatus : long
	{
		Pending = 0L,                                                                   // 0
																						// Complete = 1																'1
		ErrorOverwrite = 1L << 1,                                       // 2
		ErrorAppend = 1L << 2,                                              // 4
		ErrorRedaction = 1L << 3,                                       // 8
		ErrorBates = 1L << 4,                                               // 16
		ErrorImageCountMismatch = 1L << 5,                      // 32
		ErrorDocumentInProduction = 1L << 6,                    // 64
		NoImageSpecifiedOnLine = 1L << 7,                       // 128
		FileSpecifiedDne = 1L << 8,                                 // 256
		InvalidImageFormat = 1L << 9,                               // 512
		ColumnMismatch = 1L << 10,                                      // 1024
		EmptyFile = 1L << 11,                                               // 2048
		EmptyIdentifier = 1L << 12,                                 // 4096
		IdentifierOverlap = 1L << 13,                               // 8192
		SecurityUpdate = 1L << 14,                                      // 16384
		SecurityAdd = 1L << 15,                                         // 32768
		ErrorOriginalInProduction = 1L << 16,               // 65536
		ErrorAppendNoParent = 1L << 17,                         // 131072
		ErrorDuplicateAssociatedObject = 1L << 18,      // 262144
		SecurityAddAssociatedObject = 1L << 19,         // 524288
		ErrorAssociatedObjectIsChild = 1L << 20,            // 1048576
		ErrorAssociatedObjectIsDocument = 1L << 21, // 2097152
		ErrorOverwriteMultipleKey = 1L << 22,               // 4194304
		ErrorTags = 1L << 23,                                               // 8388608
		ErrorAssociatedObjectIsMissing = 1L << 24,      // 16777216
		DataGridInvalidDocumentIDError = 1L << 25,      // 33554432
		DataGridFieldMaxSizeExceeded = 1L << 26,            // 67108864
		DataGridInvalidFieldNameError = 1L << 27,       // 134217728
		DataGridExceptionOccurred = 1L << 28,               // 268435456
		ErrorParentMustBeFolder = 1L << 29,             // 536870912
		EmptyOverlayIdentifier = 1L << 30               // 1073741824
														// 1 << 63 is the max possible value here
	}

	public class ImportStatusHelper
	{
		private static Dictionary<ImportStatus, string> _importStatusMessages;

		public static string GetCsvErrorLine(long status, string identifier, string errorBatesIdentifier, int errorBatesArtifactID, string documentIdentifier, string dataGridException, string errorData = "")
		{
			var retval = new System.Text.StringBuilder();
			if ((status & (long)ImportStatus.ErrorAppend) > 0L)
			{
				retval.Append(ConvertToMessageLineInCell($"An item with identifier {documentIdentifier} already exists in the workspace"));
			}

			if ((status & (long)ImportStatus.ErrorBates) > 0L)
			{
				if ((errorBatesIdentifier ?? "") != (string.Empty ?? ""))
				{
					retval.Append(ConvertToMessageLineInCell("This image was not imported; the page identifier " + identifier + " already exists for " + errorBatesIdentifier));
				}
				else if (errorBatesArtifactID != -1)
				{
					retval.Append(ConvertToMessageLineInCell("This image was not imported; the page identifier " + identifier + " already exists for the document with Artifact ID " + errorBatesArtifactID));
				}
				else
				{
					retval.Append(ConvertToMessageLineInCell("This image was not imported; other images in this document have page identifiers already in use in the workspace"));
				}
			}

			foreach (KeyValuePair<ImportStatus, string> errorKvp in SimpleImportStatusErrorMessageDictionary)
			{
				if ((status & (long)errorKvp.Key) > 0L)
				{
					retval.Append(ConvertToMessageLineInCell(FormatImportStatusErrorMessage(errorKvp.Value, errorData)));
				}
			}

			if ((status & (long)Relativity.MassImport.DTO.ImportStatus.DataGridExceptionOccurred) > 0L && !string.IsNullOrEmpty(dataGridException))
				retval.Append(ConvertToMessageLineInCell(dataGridException));
			return retval.ToString().TrimEnd('\n');
		}

		private static string FormatImportStatusErrorMessage(string message, string errorData)
		{
			if (string.IsNullOrEmpty(errorData))
			{
				return message;
			}

			var @params = errorData.Split('|');
			string formattedMessage = string.Format(message, @params);
			return formattedMessage;
		}

		private static Dictionary<ImportStatus, string> SimpleImportStatusErrorMessageDictionary
		{
			get
			{
				// Note to developers (about ImportStatus.SecurityAdd): This error can also be caused by the FolderCache within WebAPI.
				// If you have recently deleted folders in Relativity and then are trying to add them again by importing documents in the RDC, 
				// and you get this error, then your problem is probably caused by folder caching.  To fix this problem, you must do an iisreset.
				// Or, wait until the cache times out.
				if (_importStatusMessages is null)
				{
					_importStatusMessages = new Dictionary<ImportStatus, string>()
					{
						{ ImportStatus.ErrorOverwrite, "This document identifier does not exist in the workspace - no document to overwrite" }, 
						{ ImportStatus.ErrorRedaction, "This document contains redactions or highlights that can't be overwritten" }, 
						{ ImportStatus.ErrorImageCountMismatch, "The number of original images and the number of imported production images for this document differ" }, 
						{ ImportStatus.ErrorDocumentInProduction, "Document is already in the selected production" }, 
						{ ImportStatus.NoImageSpecifiedOnLine, "There is no image specified on this line" }, 
						{ ImportStatus.FileSpecifiedDne, "One of the files specified for this document does not exist" }, 
						{ ImportStatus.InvalidImageFormat, "The image file specified is not a supported format in Relativity" }, 
						{ ImportStatus.ColumnMismatch, "There are an unequal number of columns on this line compared to the header line" }, 
						{ ImportStatus.EmptyFile, "The file being uploaded is empty" }, 
						{ ImportStatus.EmptyIdentifier, "The identifier field for this row is either empty or unmapped" }, 
						{ ImportStatus.IdentifierOverlap, "The identifier specified on this line has been previously specified in the file" }, 
						{ ImportStatus.SecurityUpdate, "The document specified has been secured for editing" }, 
						{ ImportStatus.SecurityAdd, "Your account does not have rights to add a document or object to this case" }, 
						{ ImportStatus.ErrorOriginalInProduction, "Document is already part of a production and original images can't be overwritten" }, 
						{ ImportStatus.ErrorAppendNoParent, "No parent artifact specified for this new object" }, 
						{ ImportStatus.ErrorDuplicateAssociatedObject, "A non unique associated object is specified for this new object" }, 
						{ ImportStatus.SecurityAddAssociatedObject, "Your account does not have rights to add an associated object to the current object" }, 
						{ ImportStatus.ErrorAssociatedObjectIsChild, "20.006. Failed to copy source field into destination field due to missing child object. Review the following destination field(s): '{0}'" }, 
						{ ImportStatus.ErrorAssociatedObjectIsDocument, "An object field references a document which does not exist. Review the following destination field(s): '{0}'" }, 
						{ ImportStatus.ErrorOverwriteMultipleKey, "This record's Overlay Identifier is shared by multiple documents in the case, and cannot be imported" }, 
						{ ImportStatus.ErrorTags, "This document contains tags that can't be overwritten" }, 
						{ ImportStatus.ErrorAssociatedObjectIsMissing, "An object field references an artifact ID which doesn't exist for the object. Review the following destination field(s): '{0}'" }, 
						{ ImportStatus.DataGridFieldMaxSizeExceeded, "This document has a Data Grid field containing data that exceeds the maximum size threshold. Relativity skipped this document's Data Grid upload." }, 
						{ ImportStatus.DataGridInvalidDocumentIDError, "The document DataGridID has an invalid value." }, 
						{ ImportStatus.DataGridInvalidFieldNameError, "The field name has an invalid value." }, 
						{ ImportStatus.ErrorParentMustBeFolder, "The parent of the document must be a folder." }, 
						{ ImportStatus.EmptyOverlayIdentifier, "The overlay identifier field for this row is empty." }
					};
				}

				return _importStatusMessages;
			}
		}

		public static string ConvertToMessageLineInCell(string message)
		{
			return string.Format(" - {0}{1}", message, '\n');
		}
	}
}