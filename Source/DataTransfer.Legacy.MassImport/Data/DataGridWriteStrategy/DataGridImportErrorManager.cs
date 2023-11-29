using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class DataGridImportErrorManager
	{
		private readonly Relativity.Data.DataGridMappingMultiDictionary _mappings;
		private readonly DataGridWriteStrategy.IDListDictionary<string> _errors;
		private readonly DataGridWriteStrategy.IDListDictionary<ImportStatus> _validationErrors;
		private readonly Dictionary<int, List<int>> _fieldErrors;
		private readonly Logging.ILog _correlationLogger;

		public DataGridImportErrorManager(Relativity.Data.DataGridMappingMultiDictionary dataGridMappings, Logging.ILog correlationLogger)
		{
			_mappings = dataGridMappings;
			_correlationLogger = correlationLogger;
			_errors = new IDListDictionary<string>();
			_validationErrors = new IDListDictionary<ImportStatus>();
			_fieldErrors = new Dictionary<int, List<int>>();
		}

		public async Task AddValidationStatuses(int artifactId, IEnumerable<Relativity.MassImport.DTO.ImportStatus> statuses)
		{
			var identity = await _mappings.LookupRecordByArtifactID(artifactId);
			foreach (ImportStatus status in statuses)
			{
				lock (_validationErrors)
				{
					_validationErrors.Add(status, (long) identity.ImportID);
				}
			}
		}

		public async Task AddErrorStatuses(IEnumerable<int> artifactIds, string errors)
		{
			foreach (int artifactId in artifactIds)
			{
				await AddErrorStatuses(artifactId, errors);
			}
		}

		public async Task AddErrorStatuses(int artifactId, string errors)
		{
			var identity = await _mappings.LookupRecordByArtifactID(artifactId);
			lock (_errors)
			{
				_correlationLogger.LogVerbose("Error: {error} with ArtifactID: {artifactId}", errors, artifactId);
				_errors.Add(errors, (long)identity.ImportID);
			}
		}

		public async Task AddFieldErrors(Dictionary<int, List<int>> fieldErrors)
		{
			foreach (KeyValuePair<int, List<int>> errorPair in fieldErrors)
			{
				lock (_fieldErrors)
				{
					if (!_fieldErrors.ContainsKey(errorPair.Key))
					{
						_fieldErrors[errorPair.Key] = new List<int>();
					}
				}

				foreach (int artifactId in errorPair.Value)
				{
					var identity = await _mappings.LookupRecordByArtifactID(artifactId);
					lock (_fieldErrors)
					{
						_fieldErrors[errorPair.Key].Add(identity.ArtifactID);
					}
				}
			}
		}

		public IDListDictionary<string> ErrorMessages => _errors;

		public IDListDictionary<ImportStatus> ValidationStatuses => _validationErrors;

		public Dictionary<int, List<int>> FieldErrors => _fieldErrors;
	}
}