using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class ImportMeasurements
	{
		public ImportMeasurements()
		{
			SqlImportTime = new Stopwatch();
			SqlBulkImportTime = new Stopwatch();
			PrimaryArtifactCreationTime = new Stopwatch();
			SecondaryArtifactCreationTime = new Stopwatch();
			DataGridImportTime = new Stopwatch();
			DataGridFileSize = 0L;

			_stopwatches = new Dictionary<string, Stopwatch>();
			_counters = new Dictionary<string, int>();
			_sqlStatistics = new Dictionary<string, long>();
		}

		public Stopwatch SqlImportTime { get; }
		public Stopwatch SqlBulkImportTime { get; }
		public Stopwatch PrimaryArtifactCreationTime { get; }
		public Stopwatch SecondaryArtifactCreationTime { get; }
		public Stopwatch DataGridImportTime { get; }
		public long DataGridFileSize { get; set; }

		private readonly Dictionary<string, Stopwatch> _stopwatches;
		private readonly Dictionary<string, int> _counters;
		private readonly Dictionary<string, long> _sqlStatistics = new Dictionary<string, long>();

		public void StartMeasure([System.Runtime.CompilerServices.CallerMemberName] string measureName = null)
		{
			if (measureName is object)
			{
				if (!_stopwatches.ContainsKey(measureName))
				{
					_stopwatches.Add(measureName, new Stopwatch());
				}

				_stopwatches[measureName].Start();
			}
		}

		public void IncrementCounter(string counterName)
		{
			if (counterName is object)
			{
				if (!_counters.ContainsKey(counterName))
				{
					_counters.Add(counterName, 0);
				}

				_counters[counterName] = _counters[counterName] + 1;
			}
		}

		public void StopMeasure([System.Runtime.CompilerServices.CallerMemberName] string measureName = null)
		{
			if (measureName is object && _stopwatches.ContainsKey(measureName))
			{
				_stopwatches[measureName].Stop();
			}
		}

		public void ParseTimeStatistics(string message)
		{
			const string SQL_SERVER_EXECUTION_TIMES = " SQL Server Execution Times:";
			const string SQL_SERVER_PARSE_AND_COMPILE_TINES = "SQL Server parse and compile time: ";
			const string ELAPSED_TIME = "elapsed time = ";
			using (var reader = new StringReader(message))
			{
				string key = null;
				long value = 0L;
				while (true)
				{
					string line = reader.ReadLine();
					if (line is null)
					{
						break;
					}

					if (line.StartsWith(PrintSectionQuery.MASS_IMPORT_SECTION))
					{
						if (key != null)
						{
							_sqlStatistics.Add(key, value);
						}

						key = line.Substring(PrintSectionQuery.MASS_IMPORT_SECTION.Length);
						value = 0L;
					}
					else if ((line ?? "") == SQL_SERVER_EXECUTION_TIMES | (line ?? "") == SQL_SERVER_PARSE_AND_COMPILE_TINES)
					{
						line = reader.ReadLine();
						int beginElapsedTime = line.LastIndexOf(ELAPSED_TIME) + ELAPSED_TIME.Length;
						int endElapsedTime = line.IndexOf(" ms", beginElapsedTime);
						value += long.Parse(line.Substring(beginElapsedTime, endElapsedTime - beginElapsedTime));
					}
				}

				if (key != null)
				{
					_sqlStatistics.Add(key, value);
				}
			}
		}

		public IEnumerable<KeyValuePair<string, long>> GetMeasures()
		{
			foreach (var x in _stopwatches)
			{
				if (x.Value.IsRunning)
				{
					yield return new KeyValuePair<string, long>(x.Key, -1);
				}
				else
				{
					yield return new KeyValuePair<string, long>(x.Key, x.Value.ElapsedMilliseconds);
				}
			}

			foreach (var x in _sqlStatistics)
				yield return x;
		}

		public IEnumerable<KeyValuePair<string, int>> GetCounters()
		{
			return _counters;
		}
	}
}