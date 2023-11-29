using System;

namespace Relativity.MassImport.DTO
{
	[Serializable]
	public class ObjectLoadInfo : NativeLoadInfo
	{
		private int _artifactTypeID;

		public int ArtifactTypeID
		{
			get => _artifactTypeID;
			set => _artifactTypeID = value;
		}
	}
}
