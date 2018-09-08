using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using static TriggersTools.IO.Windows.Native.Win32;

namespace TriggersTools.IO.Windows {
	/// <summary>
	/// Static helpers for working with Windows 10 (April 2018 update) directory case sensitivity.
	/// </summary>
	public static class DirectoryCaseSensitivity {

		#region Constants

		/// <summary>The temporary directory to use for checking support.</summary>
		private static readonly string TempDirectory = Path.Combine(Path.GetTempPath(), "TriggersToolsGames");

		#endregion

		#region Fields

		/// <summary>True if this version of Windows supports directory case sensitivity.</summary>
		private static bool? isSupported;

		#endregion

		#region IsSupported

		/// <summary>Gets if this version of Windows supports directory case sensitivity.</summary>
		/// 
		/// <returns>True if directory case sensitivity is supported.</returns>
		/// <remarks>This only needs to be calculated once per program execution.</remarks>
		/// 
		/// <exception cref="Exception">
		/// Failed to open file while checking case sensitivity support.-or-
		/// Unknown NTSTATUS while checking case sensitivity support.
		/// </exception>
		public static bool IsSupported() {
			if (isSupported.HasValue)
				return isSupported.Value;

			try {
				if (!Directory.Exists(TempDirectory))
					Directory.CreateDirectory(TempDirectory);

				IntPtr hFile = CreateFile(TempDirectory, 0, FileShare.ReadWrite,
										  IntPtr.Zero, FileMode.Open,
										  CreateFileFlags.BackupSemantics, IntPtr.Zero);
				if (hFile == InvalidHandle) {
					int error = Marshal.GetLastWin32Error();
					switch (error) {
					case ErrorCodes.FileNotFound: throw new DirectoryNotFoundException(TempDirectory);
					case ErrorCodes.AccessDenied: throw new UnauthorizedAccessException(TempDirectory);
					default: throw new Win32Exception(error);
					}
				}
				try {
					IoStatusBlock iosb = new IoStatusBlock();
					FileCaseSensitiveInformation caseSensitive = new FileCaseSensitiveInformation();
					// Strangely enough, this doesn't fail on files
					NTStatus result = NtQueryInformationFile(hFile, ref iosb, ref caseSensitive,
															 Marshal.SizeOf(typeof(FileCaseSensitiveInformation)),
															 FileInformationClass.FileCaseSensitiveInformation);
					switch (result) {
					case NTStatus.Success:
						return (isSupported = true).Value;
					case NTStatus.NotImplemented:
					case NTStatus.InvalidInfoClass:
					case NTStatus.InvalidParameter:
					case NTStatus.NotSupported:
						// Not supported, must be older version of windows.
						// Directory case sensitivity is impossible.
						return (isSupported = false).Value;
					default:
						throw new Exception($"Unknown NTSTATUS: {(uint) result:X8}!");
					}
				}
				finally {
					CloseHandle(hFile);
				}
			}
			catch (Exception ex) {
				throw new Exception("An error occurred while checking directory case sensitivity support!", ex);
			}
		}

		#endregion

		#region Case Sensitivity

		/// <summary>Gets if the specified directory is case sensitive.</summary>
		/// 
		/// <param name="directory">The path of the directory to check.</param>
		/// <returns>
		/// True if the directory is case sensitive.<para/>
		/// If this version of Windows does not support case sensitivity, the return value will always be
		/// false.
		/// </returns>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
		/// <exception cref="UnauthorizedAccessException">Failed to open the directory.</exception>
		/// <exception cref="Win32Exception">Unspecified error while opening directory.</exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static bool IsCaseSensitive(string directory) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			if (!IsSupported())
				return false;
			IntPtr hFile = CreateFile(directory, 0, FileShare.ReadWrite,
									  IntPtr.Zero, FileMode.Open,
									  CreateFileFlags.BackupSemantics, IntPtr.Zero);
			if (hFile == InvalidHandle) {
				int error = Marshal.GetLastWin32Error();
				switch (error) {
				case ErrorCodes.FileNotFound: throw new DirectoryNotFoundException(directory);
				case ErrorCodes.AccessDenied: throw new UnauthorizedAccessException(directory);
				default: throw new Win32Exception(error);
				}
			}
			try {
				IoStatusBlock iosb = new IoStatusBlock();
				FileCaseSensitiveInformation caseSensitive = new FileCaseSensitiveInformation();
				NTStatus result = NtQueryInformationFile(hFile, ref iosb, ref caseSensitive,
														 Marshal.SizeOf(typeof(FileCaseSensitiveInformation)),
														 FileInformationClass.FileCaseSensitiveInformation);
				switch (result) {
				case NTStatus.Success:
					return caseSensitive.Flags.HasFlag(CaseSensitiveFlags.CaseSensitiveDirectory);
				case NTStatus.NotImplemented:
				case NTStatus.InvalidInfoClass:
				case NTStatus.InvalidParameter:
				case NTStatus.NotSupported:
					// Not supported, must be older version of windows.
					// Directory case sensitivity is impossible.
					return false;
				default:
					throw new Exception($"Unknown NTSTATUS: {(uint) result:X8}!");
				}
			}
			finally {
				CloseHandle(hFile);
			}
		}

		/// <summary>Sets if the specified directory is case sensitive.</summary>
		/// 
		/// <param name="directory">The path of the directory to set.</param>
		/// <param name="enable">True if case sensitivity should be enabled.</param>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
		/// <exception cref="UnauthorizedAccessException">Failed to open the directory.</exception>
		/// <exception cref="Win32Exception">Unspecified error while opening directory.</exception>
		/// <exception cref="IOException">The directory contains matching case-insensitive files.</exception>
		/// <exception cref="NotSupportedException">
		/// This version of Windows does not support directory case sensitivity.
		/// </exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static void SetCaseSensitive(string directory, bool enable) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			ThrowIfNotSupported();
			IntPtr hFile = CreateFile(directory, FileAccessFlags.WriteAttributes, FileShare.ReadWrite,
									  IntPtr.Zero, FileMode.Open,
									  CreateFileFlags.BackupSemantics, IntPtr.Zero);
			if (hFile == InvalidHandle) {
				int error = Marshal.GetLastWin32Error();
				switch (error) {
				case ErrorCodes.FileNotFound: throw new DirectoryNotFoundException(directory);
				case ErrorCodes.AccessDenied: throw new UnauthorizedAccessException(directory);
				default: throw new Win32Exception(error);
				}
			}
			try {
				IoStatusBlock iosb = new IoStatusBlock();
				FileCaseSensitiveInformation caseSensitive = new FileCaseSensitiveInformation();
				if (enable)
					caseSensitive.Flags |= CaseSensitiveFlags.CaseSensitiveDirectory;
				NTStatus result = NtSetInformationFile(hFile, ref iosb, ref caseSensitive,
													   Marshal.SizeOf(typeof(FileCaseSensitiveInformation)),
													   FileInformationClass.FileCaseSensitiveInformation);
				switch (result) {
				case NTStatus.Success:
					return;
				case NTStatus.DirectoryNotEmpty:
					throw new IOException($"Cannot set case sensitivity because directory \"{directory}\" contains files with the same case-insensitive name!");
				case NTStatus.NotImplemented:
				case NTStatus.InvalidInfoClass:
				case NTStatus.InvalidParameter:
				case NTStatus.NotSupported:
					// Not supported, must be older version of windows.
					// Directory case sensitivity is impossible.
					ThrowIfNotSupported(true);
					return;
				default:
					throw new Exception($"Unknown NTSTATUS: {(uint) result:X8}!");
				}
			}
			finally {
				CloseHandle(hFile);
			}
		}

		#endregion

		#region Inhertit

		/// <summary>Sets the directories case-sensitivity to that of its parent.</summary>
		/// 
		/// <param name="directory">The path of the directory to inherit.</param>
		/// <returns>True if the inherited directory was case sensitive.</returns>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
		/// <exception cref="UnauthorizedAccessException">Failed to open the directory.</exception>
		/// <exception cref="Win32Exception">Unspecified error while opening directory.</exception>
		/// <exception cref="IOException">The directory contains matching case-insensitive files.</exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static bool Inherit(string directory) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			string parent = Path.GetDirectoryName(directory);
			if (string.IsNullOrEmpty(parent))
				throw new ArgumentException($"Cannot inherit, directory \"{directory}\" is root!");
			if (IsSupported()) {
				bool enabled = IsCaseSensitive(parent);
				SetCaseSensitive(directory, enabled);
				return enabled;
			}
			return false;
		}

		#endregion

		#region Create

		/// <summary>Creates a directory and sets its case sensitivity.</summary>
		/// 
		/// <param name="directory">The path of the directory to create and set.</param>
		/// <param name="enable">True if case sensitivity should be enabled.</param>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
		/// <exception cref="UnauthorizedAccessException">Failed to open the directory.</exception>
		/// <exception cref="Win32Exception">Unspecified error while opening directory.</exception>
		/// <exception cref="NotSupportedException">
		/// This version of Windows does not support directory case sensitivity.
		/// </exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static void Create(string directory, bool enabled) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			string parent = Path.GetDirectoryName(directory);
			if (string.IsNullOrEmpty(parent))
				throw new ArgumentException($"Cannot create root directory \"{directory}\"!");
			if (enabled)
				ThrowIfNotSupported();
			Directory.CreateDirectory(directory);
			// Created directories always have this setting as off, nothing to change if disabled
			if (enabled)
				SetCaseSensitive(directory, enabled);
		}

		/// <summary>Creates a directory and inherits case sensitivity from its parent.</summary>
		/// 
		/// <param name="directory">The path of the directory to create and set.</param>
		/// <param name="enable">True if case sensitivity should be enabled.</param>
		/// <returns>True if the inherited directory was case sensitive.</returns>
		/// 
		/// <exception cref="ArgumentNullException"><paramref name="directory"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
		/// <exception cref="UnauthorizedAccessException">Failed to open the directory.</exception>
		/// <exception cref="Win32Exception">Unspecified error while opening directory.</exception>
		/// <exception cref="Exception">Unknown NTSTATUS result.</exception>
		public static bool CreateInherit(string directory) {
			if (directory == null)
				throw new ArgumentNullException(nameof(directory));
			string parent = Path.GetDirectoryName(directory);
			if (string.IsNullOrEmpty(parent))
				throw new ArgumentException($"Cannot create root directory \"{directory}\"!");
			bool enabled = IsCaseSensitive(parent);
			Directory.CreateDirectory(directory);
			// Created directories always have this setting as off, nothing to change if disabled
			if (enabled)
				SetCaseSensitive(directory, enabled);
			return enabled;
		}

		#endregion

		#region ThrowIf

		/// <summary>Throws an exception if directory case sensitivity is not supported.</summary>
		/// 
		/// <param name="force">Forces the exception even if it is supported.</param>
		private static void ThrowIfNotSupported(bool force = false) {
			if (force || !IsSupported())
				throw new NotSupportedException("This version of Windows does not support directory case sensitivity!");
		}

		#endregion
	}
}
