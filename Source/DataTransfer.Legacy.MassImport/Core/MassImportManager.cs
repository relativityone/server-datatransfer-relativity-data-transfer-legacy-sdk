﻿using System;
using kCura.Utility;
using Relativity.Telemetry.APM;
using Relativity.MassImport;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

// TODO: adjust namespace with Relativity, join with Api/MassImportManager https://jira.kcura.com/browse/REL-482642
namespace Relativity.Core.Service
{
	public class MassImportManager : MassImportManagerBase
	{
		private bool CollectIDsOnCreate { get; set; }
		private Lazy<MassImportManagerNew> _massImportManagerLazy;

		public const string TEMP_TABLE_PREFIX_FOR_FOLDER_CREATION = "RELNATTMP_";

		public MassImportManager(bool collectIDsOnCreate = false)
		{
			CollectIDsOnCreate = collectIDsOnCreate;
			_massImportManagerLazy = new Lazy<MassImportManagerNew>(() => new MassImportManagerNew(new LockHelper(new AppLockProvider()), collectIDsOnCreate));
		}

		protected IAPM APMClient
		{
			get
			{
				return Client.APMClient;
			}
		}

		protected override MassImportResults AttemptRunImageImport(Core.BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, bool inRepository, Timekeeper timekeeper, MassImportResults retval)
		{
			return ConvertResults(_massImportManagerLazy.Value.AttemptRunImageImport(context, settings, inRepository, timekeeper, retval));
		}

		protected override MassImportResults AttemptRunProductionImageImport(Core.BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, int productionArtifactID, bool inRepository, MassImportResults retval)
		{
			return ConvertResults(_massImportManagerLazy.Value.AttemptRunProductionImageImport(context, settings, productionArtifactID, inRepository, retval));
		}
		
		public ErrorFileKey GenerateImageErrorFiles(Core.ICoreContext icc, string runID, int caseArtifactID, bool writeHeader, int keyFieldID)
		{
			return _massImportManagerLazy.Value.GenerateImageErrorFiles(icc, runID, caseArtifactID, writeHeader, keyFieldID);
		}
		
		public bool ImageRunHasErrors(Core.ICoreContext icc, string runId)
		{
			return _massImportManagerLazy.Value.ImageRunHasErrors(icc, runId);
		}

		protected override MassImportResults AttemptRunNativeImport(Core.BaseContext context, Relativity.MassImport.DTO.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, Timekeeper timekeeper, MassImportResults retval)
		{
			return ConvertResults(_massImportManagerLazy.Value.AttemptRunNativeImport(context, settings, inRepository, includeExtractedTextEncoding, timekeeper, retval));
		}
		
		protected override MassImportResults AttemptRunObjectImport(Core.BaseContext context, Relativity.MassImport.DTO.ObjectLoadInfo settings, bool inRepository, MassImportResults retval)
		{
			return ConvertResults(_massImportManagerLazy.Value.AttemptRunObjectImport(context, settings, inRepository, retval));
		}
		
		public ErrorFileKey GenerateNonImageErrorFiles(Core.ICoreContext icc, string runID, int artifactTypeID, bool writeHeader, int keyFieldID)
		{
			return _massImportManagerLazy.Value.GenerateNonImageErrorFiles(icc, runID, artifactTypeID, writeHeader, keyFieldID);
		}

		/// <summary>
		/// Return a SqlDataReader containing errors from a mass import operation.  It is important to close
		/// the context's connection when you are through using the reader.
		/// </summary>
		/// <param name="context">A RowDataContext.BaseContext object.  It is important to call ReleaseConnection()
		/// on this object when you are done with the reader</param>
		/// <param name="runID"></param>
		/// <param name="keyArtifactID"></param>
		/// <returns></returns>
		/// <remarks>Historical Note: The reason we don't pass in a Relativity.Core.ICoreContext is because then,
		/// the method would need to internally generate a kCura.Data.RowDataGateway.BaseContext, which may create
		/// a new DataContext.  When creating the reader, we would be opening a connection on that context.
		/// Then we would return the reader.  At that point, the caller would not be able to close the connection</remarks>
		public System.Data.SqlClient.SqlDataReader GenerateNativeErrorReader(kCura.Data.RowDataGateway.BaseContext context, string runID, int keyArtifactID)
		{
			return _massImportManagerLazy.Value.GenerateNativeErrorReader(context, runID, keyArtifactID);
		}

		public bool NativeRunHasErrors(Core.ICoreContext icc, string runId)
		{
			return _massImportManagerLazy.Value.NativeRunHasErrors(icc, runId);
		}

		public object DisposeRunTempTables(Core.ICoreContext icc, string runId)
		{
			return _massImportManagerLazy.Value.DisposeRunTempTables(icc, runId);
		}

		public bool AuditImport(Core.BaseServiceContext icc, string runID, bool isFatalError, Relativity.MassImport.DTO.ImportStatistics importStats)
		{
			return _massImportManagerLazy.Value.AuditImport(icc, runID, isFatalError, importStats);
		}
		
		public bool HasImportPermission(Core.ICoreContext context)
		{
			return Core.PermissionsHelper.HasAdminOperationPermission(context, Core.Permission.AllowDesktopClientImport);
		}

		public MassImportResults PostImportDocumentLimitLogic(Core.BaseServiceContext sc, int workspaceId, MassImportResults importResults)
		{
			return ConvertResults(_massImportManagerLazy.Value.PostImportDocumentLimitLogic(sc, workspaceId, importResults));
		}

		private MassImportResults ConvertResults(MassImportManagerBase.MassImportResults results)
		{
			return results is MassImportManagerBase.DetailedMassImportResults ?
				new MassImportManagerBase.DetailedMassImportResults(results as MassImportManagerBase.DetailedMassImportResults) :
				new MassImportManagerBase.MassImportResults(results);
		}
	}
}