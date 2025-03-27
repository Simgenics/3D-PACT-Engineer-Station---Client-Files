#nullable enable
namespace LMS.Models
{
	public class FileQuery
	{
		/// <summary>
		/// Valid containers: "assets", "scenario", "scenes", "resources"
		/// </summary>
		public string? container { get; set; }
		
		/// <summary>
		/// Name of the file to be downloaded
		/// e.g. "scenario_20230202T101700323.json"
		/// </summary>
		public string? file { get; set; }

		/// <summary>
		/// Version of the file used for caching
		/// </summary>
		public uint? bundleID { get; set; }
	}
}
