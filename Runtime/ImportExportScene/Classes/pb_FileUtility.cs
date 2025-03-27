using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ImportExportScene
{
    public static class pb_FileUtility
    {

		public static bool SaveFile(string path, string contents)
		{
			try
			{
				File.WriteAllText(path, contents);
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed writing to path: " + path + "\n" + e.ToString());
				return false;
			}

			return true;
		}

		public static bool IsValidPath(string path, string extension)
		{
			return !string.IsNullOrEmpty(path) &&
				System.Uri.IsWellFormedUriString(path, System.UriKind.RelativeOrAbsolute) &&
				path.EndsWith(extension);
		}

		/**
		 * Given a string, this function attempts to extrapolate the absolute path using current directory as the
		 * relative root.
		 */
		public static string GetFullPath(string path)
		{
			string full = Path.GetFullPath(path);
			return full;
		}

		/**
		 * Replace backslashes with forward slashes, and make sure that path is the full path.
		 */
		public static string SanitizePath(string path, string extension = null)
		{
			string rep = GetFullPath(path);
			// @todo On Windows this defaults to '\', but doesn't escape correctly.
			// Path.DirectorySeparatorChar.ToString());
			rep = Regex.Replace(rep, "(\\\\|\\\\\\\\){1,2}|(/)", "/");
			// white space gets the escaped symbol
			rep = Regex.Replace(rep, "\\s", "%20");

			if (extension != null && !rep.EndsWith(extension))
			{
				if (!extension.StartsWith("."))
					extension = "." + extension;

				rep += extension;
			}

			return rep;
		}
	}
}
