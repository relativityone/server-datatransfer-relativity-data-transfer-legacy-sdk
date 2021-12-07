using System;
using System.Linq;

namespace Relativity.MassImport.Extensions
{
	internal static class NativeLoadInfoExtensions
	{
		public static FieldInfo GetKeyField(this NativeLoadInfo settings)
		{
			settings = settings ?? throw new ArgumentNullException(nameof(settings));
			return settings.MappedFields?.FirstOrDefault(f => f.ArtifactID == settings.KeyFieldArtifactID);
		}
	}
}
