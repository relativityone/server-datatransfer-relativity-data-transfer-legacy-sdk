using System;
using System.Collections.Generic;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class Artifact
	{
		public Artifact()
		{
			Keywords = string.Empty;
			Notes = string.Empty;
			TextIdentifier = string.Empty;
			Guids = new List<Guid>();
		}

		public int ArtifactID { get; set; }

		public int ArtifactTypeID { get; set; }

		public int? ParentArtifactID { get; set; }

		public int? ContainerID { get; set; }

		public int AccessControlListID { get; set; }

		public bool AccessControlListIsInherited { get; set; }

		public string Keywords { get; set; }

		public string Notes { get; set; }

		public string TextIdentifier { get; set; }

		public DateTime LastModifiedOn { get; set; }

		public int LastModifiedBy { get; set; }

		public int CreatedBy { get; set; }

		public DateTime CreatedOn { get; set; }

		public bool DeleteFlag { get; set; }

		public List<Guid> Guids { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}