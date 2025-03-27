namespace SharedAssets
{
	public static class PlatformUtil
	{
		public static string GetCurrentPlatform()
		{
			var platform = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		platform = "StandaloneWindows64";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        platform = "StandaloneOSX";
#elif UNITY_WEBGL
			platform = "WebGL";
#endif
			return platform;
		}
	}
}