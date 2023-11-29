using System;

namespace Relativity.MassImport.Core
{
	public class MassImportManagerLockKey : IEquatable<MassImportManagerLockKey>
	{

		// We need to use the same lock for Document and Image because importing an image
		// may cause a document to be created
		public enum LockType
		{
			Objects,
			Choice,
			Folder,
			DocumentOrImageOrProductionImage,
			SingleObjectField,
			MultiObjectField
		}

		// OperationType and WorkspaceID are immutable because
		// this object is intended as a dictionary key.  Therefore it is best if
		// the properties used to compute the hash key are immutable.
		private LockType _operationType;

		public LockType OperationType
		{
			get
			{
				return _operationType;
			}
		}

		private int _workspaceArtifactID;

		public int WorkspaceArtifactID
		{
			get
			{
				return _workspaceArtifactID;
			}
		}

		public MassImportManagerLockKey(int workspaceArtifactID, LockType operationType)
		{
			_workspaceArtifactID = workspaceArtifactID;
			_operationType = operationType;
		}

		public override bool Equals(object other)
		{
			// Check Nothing
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			// Check if this is the same instance of the object
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			// Type must match, otherwise the objects are not equal
			if (!(other is MassImportManagerLockKey))
			{
				return false;
			}

			// Check that the property values match
			return HasEqualProperties((MassImportManagerLockKey)other);
		}

		public bool Equals(MassImportManagerLockKey other)
		{
			// Check Nothing
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			// Check if this is the same instance of the object
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			// Check that the property values match
			return HasEqualProperties(other);
		}

		private bool HasEqualProperties(MassImportManagerLockKey other)
		{
			return WorkspaceArtifactID == other.WorkspaceArtifactID && OperationType == other.OperationType;
		}

		public override int GetHashCode()
		{
			return WorkspaceArtifactID ^ (int)OperationType;
		}
	}
}