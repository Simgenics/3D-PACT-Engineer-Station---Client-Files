using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace ImportExportScene
{
    public static class pb_Config
    {
		public const int ASSET_MENU_ORDER = 800;

		/**
		 * When saving and loading levels using the Resources folder, the following subfolders will
		 * searched for assets.
		 */
		public static readonly string[] Resource_Folder_Paths = new string[]
		{
			"Level Editor Prefabs",
			"EJJV",
		};

		/**
		 * When saving an loading levels using AssetBundles, these are the names that will be automatically
		 * added to the pb_AssetBundles available bundles list.  You may add additional asset bundles at
		 * runtime using `pb_AssetBundles.RegisterAssetBundle()`.
		 */
		public static readonly string[] AssetBundle_Names = new string[]
		{
			"TestAssets"
		};

		/**
		 * When loading AssetBundle_Names, these are the directories that will be scanned for matching files.
		 */
		public static readonly string[] AssetBundle_SearchDirectories = new string[]
		{
			"Assets/AssetBundles/"
		};
	}
}
