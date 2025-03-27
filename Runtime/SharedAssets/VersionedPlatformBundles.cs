using System;

namespace SharedAssets
{
	[Serializable, Obsolete("LMS handles asset bundles")]
	public class VersionedPlatformBundles
	{
		public int Version;
		public string AssetBundleName;
		public DateTime CreatedDate;
		
		public PlatformPaths winPaths;
		public PlatformPaths webglPaths;
		public PlatformPaths macPaths;
	}

	[Serializable]
	public class PlatformPaths
	{
		public string mainBundle;
		public string[] dependencies;
	}
}
