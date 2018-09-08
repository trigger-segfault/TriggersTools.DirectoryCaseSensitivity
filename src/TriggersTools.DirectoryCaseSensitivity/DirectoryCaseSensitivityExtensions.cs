using System;
using System.ComponentModel;
using System.IO;

namespace TriggersTools.IO.Windows {
	/// <summary>
	/// Extension methods for <see cref="DirectoryCaseSensitivity"/> and <see cref="DirectoryInfo"/>.
	/// </summary>
	public static class DirectoryCaseSensitivityExtensions {

		#region DirectoryInfo

		/// <summary>Gets if the specified directory is case sensitive.</summary>
		/// 
		/// <param name="dirInfo">The directory info to check.</param>
		/// <returns>
		/// True if the directory is case sensitive.<para/>
		/// If this version of Windows does not support case sensitivity, the return value will always be
		/// false.
		/// </returns>
		/// 
		/// <exception cref="Win32Exception">Error while opening directory.</exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static bool IsCaseSensitive(this DirectoryInfo dirInfo) {
			return DirectoryCaseSensitivity.IsCaseSensitive(dirInfo.FullName);
		}

		/// <summary>Sets if the specified directory is case sensitive.</summary>
		/// 
		/// <param name="dirInfo">The directory info to set.</param>
		/// <param name="enable">True if case sensitivity should be enabled.</param>
		/// 
		/// <exception cref="Win32Exception">Error while opening directory.</exception>
		/// <exception cref="NotSupportedException">
		/// This version of Windows does not support directory case sensitivity.
		/// </exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static void SetCaseSensitive(this DirectoryInfo dirInfo, bool enable) {
			DirectoryCaseSensitivity.SetCaseSensitive(dirInfo.FullName, enable);
		}

		#endregion
	}
}
