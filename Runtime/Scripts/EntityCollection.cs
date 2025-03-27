using System.Collections.Generic;

namespace LMS.Models
{
	/// <summary>
	/// This is to be used for AssetBundles, Scenes, Scenarios, and any other entity that has a paged result
	/// </summary>
	public abstract class EntityCollection<T>
	{
		public int totalCount { get; set; }
		public List<T> items { get; set; }
	}
}
