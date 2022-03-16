using System;
using System.Collections.Generic;
using Relativity.Logging;

// TODO: adjust namespace with Relativity, join with Api/MassImportManager https://jira.kcura.com/browse/REL-482642
namespace Relativity.Core.Service
{
	public abstract class MassImportManagerBase
	{
		protected abstract MassImportResults AttemptRunImageImport(Core.BaseContext context, ImageLoadInfo settings, bool inRepository, kCura.Utility.Timekeeper timekeeper, MassImportResults retval);
		protected abstract MassImportResults AttemptRunProductionImageImport(Core.BaseContext context, ImageLoadInfo settings, int productionArtifactID, bool inRepository, MassImportResults retval);
		protected abstract MassImportResults AttemptRunNativeImport(Core.BaseContext context, NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string bulkFileSharePath, kCura.Utility.Timekeeper timekeeper, MassImportResults retval);
		protected abstract MassImportResults AttemptRunObjectImport(Core.BaseContext context, ObjectLoadInfo settings, bool inRepository, MassImportResults retval);

		public virtual int GetCaseAuditUserId(Core.BaseContext context, string onBehalfOfUserToken)
		{
			int retval = context.UserID;
			const int EddsMasterId = -1;

			if (!string.IsNullOrWhiteSpace(onBehalfOfUserToken))
			{
				int masterUserId = Service.RelativityServicesAuthenticationTokenManager.GetUserIdFromTokenForAuditSpoofing(onBehalfOfUserToken);

				if (masterUserId == EddsMasterId)
				{
					throw new System.Exception("Invalid token supplied.");
				}
				else
				{
					Service.RelativityServicesAuthenticationTokenManager.UpdateLastTouchedTimeForTokenForAuditSpoofing(onBehalfOfUserToken);
				}

				retval = Core.UserCaseUserCache.GetLocalUserID(context.AppArtifactID, masterUserId);
			}

			return retval;
		}

		private ILog _logger;

		protected ILog CorrelationLogger
		{
			get
			{
				if (_logger is null)
				{
					_logger = Log.Logger.ForContext("CorrelationID", Guid.NewGuid(), true);
				}

				return _logger;
			}

			set
			{
				_logger = value;
			}
		}

		private MassImportResults AttemptRun(Func<MassImportResults, kCura.Utility.Timekeeper, MassImportResults> f)
		{
			var timekeeper = new Service.MassImport.VerboseLoggingTimeKeeper(CorrelationLogger);
			var retval = new MassImportResults();
			try
			{
				retval = f(retval, timekeeper);
			}
			catch (System.Exception ex)
			{
				retval.ExceptionDetail = new SoapExceptionDetail(ex);
				retval.ExceptionDetail.Details.Add("RunID:" + retval.RunID);
				var logger = Log.Logger.ForContext<MassImportManagerBase>();
				logger.LogError(ex, "MassImportManager.AttemptRun Failure");
			}

			return retval;
		}

		protected virtual int GetCaseSystemArtifactID(Core.BaseContext context)
		{
			int caseSystemArtifactID = Core.SystemArtifactCache.Instance.RetrieveArtifactIDByIdentifier(context, Core.SystemArtifact.System);
			return caseSystemArtifactID;
		}

		public MassImportResults RunImageImport(Core.ICoreContext icc, ImageLoadInfo settings, bool inRepository)
		{
			return AttemptRun((results, timekeeper) => this.AttemptRunImageImport(icc.ChicagoContext, settings, inRepository, timekeeper, results));
		}

		public MassImportResults RunProductionImageImport(Core.ICoreContext icc, ImageLoadInfo settings, int productionArtifactID, bool inRepository)
		{
			return AttemptRun((results, timekeeper) => this.AttemptRunProductionImageImport(icc.ChicagoContext, settings, productionArtifactID, inRepository, results));
		}

		public MassImportResults RunNativeImport(Core.ICoreContext icc, NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string bulkFileSharePath = null)
		{
			return AttemptRun((results, timekeeper) => this.AttemptRunNativeImport(icc.ChicagoContext, settings, inRepository, includeExtractedTextEncoding, bulkFileSharePath, timekeeper, results));
		}

		public MassImportResults RunObjectImport(Core.ICoreContext icc, ObjectLoadInfo settings, bool inRepository)
		{
			return AttemptRun((results, timekeeper) => this.AttemptRunObjectImport(icc.ChicagoContext, settings, inRepository, results));
		}

		[Serializable()]
		public class MassImportResults
		{
			public int FilesProcessed = 0;
			public int ArtifactsCreated = 0;
			public int ArtifactsUpdated = 0;
			public SoapExceptionDetail ExceptionDetail = null;
			// TODO:            Public ParentArtifactsCreated As Int32 = 0
			public string RunID;

			public MassImportResults() { }

			public MassImportResults(MassImportManagerBase.MassImportResults source)
			{
				FilesProcessed = source.FilesProcessed;
				ArtifactsCreated = source.ArtifactsCreated;
				ArtifactsUpdated = source.ArtifactsUpdated;
				ExceptionDetail = source.ExceptionDetail;
				RunID = source.RunID;
			}
		}

		[Serializable()]
		public class DetailedMassImportResults : MassImportResults
		{
			public int[] AffectedIDs { get; set; }
			public Dictionary<string, List<int>> KeyFieldToArtifactIDMapping { get; set; }

			public DetailedMassImportResults() { }

			public DetailedMassImportResults(MassImportManagerBase.DetailedMassImportResults source)
			{
				FilesProcessed = source.FilesProcessed;
				ArtifactsCreated = source.ArtifactsCreated;
				ArtifactsUpdated = source.ArtifactsUpdated;
				ExceptionDetail = source.ExceptionDetail;
				RunID = source.RunID;
				AffectedIDs = source.AffectedIDs;
				KeyFieldToArtifactIDMapping = source.KeyFieldToArtifactIDMapping;
			}
		}
	}
}