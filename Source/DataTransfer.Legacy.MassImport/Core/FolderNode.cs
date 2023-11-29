using System;
using System.Collections.Generic;
using System.Linq;

namespace Relativity.MassImport.Core
{
	internal class FolderNode
	{
		public FolderNode() : this(Enumerable.Range(1, int.MaxValue).GetEnumerator(), string.Empty, null)
		{
		}

		private FolderNode(IEnumerator<int> sequence, string name, FolderNode parent)
		{
			Sequence = sequence;
			Sequence.MoveNext();
			TempArtifactID = -Sequence.Current;
			Name = name;
			Parent = parent;
		}

		public int TempArtifactID { get; private set; }
		public FolderNode Parent { get; private set; }
		public string Name { get; private set; }
		public List<int> LeafIDs { get; private set; } = new List<int>();

		// We use both a list and dictionary to preserve the order in which the nodes were added and avoid O(n^2) insertion time
		public List<FolderNode> Nodes { get; private set; } = new List<FolderNode>();
		private Dictionary<string, FolderNode> NodeDictionary { get; set; } = new Dictionary<string, FolderNode>(CollationStringComparer.SQL_Latin1_General_CP1_CI_AS);

		// This sequence guarantees that all the TempArtifactIDs are unique within the folder tree
		// because the reference to it is shared among all nodes in that tree.
		private IEnumerator<int> Sequence { get; set; }

		public void Add(string path, int leafID)
		{
			if (path is null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			var parent = this;
			var folderNames = path
				.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.TrimEnd())
				.Where(p => !string.IsNullOrWhiteSpace(p));

			foreach (string folderName in folderNames)
			{
				FolderNode child = null;
				if (!parent.NodeDictionary.TryGetValue(folderName, out child))
				{
					child = new FolderNode(Sequence, folderName, parent);
					parent.Nodes.Add(child);
					parent.NodeDictionary.Add(folderName, child);
				}

				parent = child;
			}

			parent.LeafIDs.Add(leafID);
		}

		public IEnumerable<FolderNode> Descendants()
		{
			return DescendantsMany().SelectMany(p => p);
		}

		private IEnumerable<IEnumerable<FolderNode>> DescendantsMany()
		{
			yield return Nodes;
			foreach (FolderNode folderNode in Nodes)
			{
				yield return folderNode.Descendants();
			}
		}
	}
}