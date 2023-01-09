using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.NUnit.Core
{
	public class FolderNodeTests
	{
		[Test]
		public void NullPathThrows()
		{
			var node = new Relativity.MassImport.Core.FolderNode();
			
			Assert.Throws<ArgumentNullException>(() => node.Add(null, 10), "Null path didn't throw.");
		}

		[Test]
		public void SinglePath()
		{
			const int leadID = 10;
			var node = new Relativity.MassImport.Core.FolderNode();

			node.Add("A", leadID);

			IEnumerable<Relativity.MassImport.Core.FolderNode> descendants = node.Descendants();

			Assert.AreEqual(1, descendants.Count(), "Incorrect number of descendents.");
			Assert.AreEqual(leadID, descendants.FirstOrDefault()?.LeafIDs.FirstOrDefault() ?? 0, "Wrong leafID.");
		}

		[Test]
		public void MultipleNodesPath()
		{
			var node = new Relativity.MassImport.Core.FolderNode();

			node.Add(@"A\B", 10);
			node.Add(@"A\C", 11);

			IEnumerable<Relativity.MassImport.Core.FolderNode> descendants = node.Descendants();

			string concatenatedFolderNames = descendants.Aggregate("", (current, next) => current + next.Name);
			Assert.AreEqual(3, descendants.Count(), "Incorrect number of descendents.");
			Assert.AreEqual(2, descendants.SelectMany(p => p.LeafIDs).Count(), "Incorrect number of leafIDs.");
			Assert.AreEqual("ABC", concatenatedFolderNames, "Folder names don't match");
		}

		[Test]
		public void MultipleNodesPathCaseInsensitive()
		{
			var node = new Relativity.MassImport.Core.FolderNode();

			node.Add(@"a\B", 10);
			node.Add(@"A\C", 11);

			IEnumerable<Relativity.MassImport.Core.FolderNode> descendants = node.Descendants();

			string concatenatedFolderNames = descendants.Aggregate("", (current, next) => current + next.Name);
			Assert.AreEqual(3, descendants.Count(), "Incorrect number of descendents.");
			Assert.AreEqual(2, descendants.SelectMany(p => p.LeafIDs).Count(), "Incorrect number of leafIDs.");
			Assert.AreEqual("aBC", concatenatedFolderNames, "Folder names don't match");
		}

		[Test]
		public void SqlDataRecordEnumerable_FolderCandidates()
		{
			var node = new Relativity.MassImport.Core.FolderNode();

			node.Add(@"a", 10);
			node.Add(@"A\C", 11);

			IEnumerable<Tuple<int, int, string>> actual = node.Descendants().GetFolderCandidates().Select(p => Tuple.Create(p.GetInt32(0), p.GetInt32(1), p.GetString(2)));


			IEnumerable<Tuple<int, int, string>> expected = new[] {Tuple.Create(-2, -1, "a"), Tuple.Create(-3, -2, "C")};
			CollectionAssert.AreEqual(expected, actual, "The folder candidates do not match.");
		}

		[Test]
		public void SqlDataRecordEnumerable_GetImportMapping()
		{
			var node = new Relativity.MassImport.Core.FolderNode();

			node.Add(@"a", 10);
			node.Add(@"A\C", 11);

			IEnumerable<Relativity.MassImport.Core.FolderNode> folderNodes = node.Descendants();
			IEnumerable<FolderArtifactIDMapping> mappings =
				folderNodes.Select(p => new FolderArtifactIDMapping(p.TempArtifactID, -p.TempArtifactID, true));
			IEnumerable<Tuple<int, int>> actual = folderNodes.GetImportMapping(mappings).Select(p => Tuple.Create(p.GetInt32(0), p.GetInt32(1)));

			IEnumerable<Tuple<int, int>> expected = new[] {Tuple.Create(10, 2), Tuple.Create(11, 3)};
			CollectionAssert.AreEqual(expected, actual, "The folder mappings do not match.");
		}
	}
}
