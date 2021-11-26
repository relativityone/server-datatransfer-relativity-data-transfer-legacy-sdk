using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MassImport.NUnit.Integration.Helpers
{
	public enum AuditAction
	{
		/// <summary>
		/// Default, none audit action.
		/// </summary>
		None = 0,
		/// <summary>
		/// Create audit action.
		/// </summary>
		Create = 2,
		/// <summary>
		/// Move audit action.
		/// </summary>
		Move = 11,
		/// <summary>
		/// Import audit action.
		/// </summary>
		Import = 32,
		/// <summary>
		/// Export audit action.
		/// </summary>
		Export = 33,
	}

	public class AuditHelper
	{
		public static int GetLastRelevantAuditId(
			TestWorkspace testWorkspace,
			AuditAction action,
			int userId)
		{
			int auditId;
			
			using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText =
						$@"SELECT TOP 1 [ID]
						 FROM [EDDSDBO].[AuditRecord_PrimaryPartition]
						 WHERE [Action] = '{(int)action}'AND [Details] <> '' AND [UserID] = '{userId}'
						 ORDER BY [TimeStamp] DESC";

					auditId = Convert.ToInt32(command.ExecuteScalar());
				}
				connection.Close();
			}

			return auditId;
		}
		
		public static IEnumerable<Dictionary<string, string>> GetAuditDetails(
			TestWorkspace testWorkspace,
			AuditAction action,
			int largerThanThisAuditId,
			int nrOfLastAuditsToTake,
			int userId)
		{
			List<Dictionary<string, string>> audit = new List<Dictionary<string, string>>();
			using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText =
						$@"SELECT TOP {nrOfLastAuditsToTake} [ArtifactID], [Details] 
						 FROM [EDDSDBO].[AuditRecord_PrimaryPartition]
						 WHERE [Action] = '{(int)action}'AND [Details] <> '' AND [UserID] = '{userId}'
						 AND [ID] > '{largerThanThisAuditId}'
						 ORDER BY [TimeStamp] DESC";
					
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var singleAudit = XmlToDictionary(action, reader["Details"].ToString());
							singleAudit["ArtifactID"] = reader["ArtifactID"].ToString();

							audit.Add(singleAudit);
						}
					}
				}
				connection.Close();
			}
			return audit;
		}
		
		private static Dictionary<string, string> XmlToDictionary(AuditAction action, string data)
		{
			XElement rootElement = XElement.Parse(data);

			IEnumerable<string> names, values;

			switch (action)
			{
				case AuditAction.Export:
					names = rootElement.Element("export").Elements("item").Attributes("name").Select(n => n.Value);
					values = rootElement.Element("export").Elements("item").Select(v => v.Value);
					break;
				case AuditAction.Import:
					names = rootElement.Element("import").Elements("item").Attributes("name").Select(n => n.Value);
					values = rootElement.Element("import").Elements("item").Select(v => v.Value);
					break;
				case AuditAction.Move:
					names = rootElement.Element("moveDetails").Attributes().Select(n => n.Name.ToString());
					values = rootElement.Element("moveDetails").Attributes().Select(v => v.Value);
					break;
				default:
					return new Dictionary<string, string>();
			}

			var list = names.Zip(values, (k, v) => new { k, v }).ToDictionary(item => item.k, item => item.v);
			return list;
		}
	}
}
