using System;
using ImportExportScene.Serialization;
using LMS.Models;

namespace SharedAssets
{
	public class SceneConfig
	{
		public Guid id;
		public SceneDto lmsScene;
		public AssetBundleDto lmsAssetBundle;
		public pb_SceneNode initialCondition;
	}
}
