using System;

namespace LMS.Models
{
	/// <summary>
	/// LMS entities that share the same properties
	/// </summary>
	public class EntityDto
	{
		#nullable enable
		public int id;
		public string? name;
		public DateTime creationTime;
	}
	
	public enum StatusState
	{
		Unlocked,
		Locked
	}
}
