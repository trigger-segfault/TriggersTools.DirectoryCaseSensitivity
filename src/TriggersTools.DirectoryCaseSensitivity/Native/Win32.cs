using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TriggersTools.IO.Windows.Native {
	/// <summary>A static class for native methods, structs, and enums.</summary>
	internal static class Win32 {

		/// <summary>An invalid handle value of -1 returned from some functions.</summary>
		public static readonly IntPtr InvalidHandle = new IntPtr(-1);

		/// <summary>Status results for NT operations.</summary>
		public enum NTStatus : uint {
			Success = 0x00000000,
			NotImplemented = 0xC0000002,
			InvalidInfoClass = 0xC0000003,
			InvalidParameter = 0xC000000D,
			AccessDenied = 0xC0000022,
			NotSupported = 0xC00000BB,
			DirectoryNotEmpty = 0xC0000101,
		}

		/// <summary>
		/// A value that specifies which structure to use to query or set information for a file object.
		/// </summary>
		public enum FileInformationClass {
			None = 0,
			FileDirectoryInformation = 1,
			FileFullDirectoryInformation,
			FileBothDirectoryInformation,
			FileBasicInformation,
			FileStandardInformation,
			FileInternalInformation,
			FileEaInformation,
			FileAccessInformation,
			FileNameInformation,
			FileRenameInformation,
			FileLinkInformation,
			FileNamesInformation,
			FileDispositionInformation,
			FilePositionInformation,
			FileFullEaInformation,
			FileModeInformation,
			FileAlignmentInformation,
			FileAllInformation,
			FileAllocationInformation,
			FileEndOfFileInformation,
			FileAlternateNameInformation,
			FileStreamInformation,
			FilePipeInformation,
			FilePipeLocalInformation,
			FilePipeRemoteInformation,
			FileMailslotQueryInformation,
			FileMailslotSetInformation,
			FileCompressionInformation,
			FileObjectIdInformation,
			FileCompletionInformation,
			FileMoveClusterInformation,
			FileQuotaInformation,
			FileReparsePointInformation,
			FileNetworkOpenInformation,
			FileAttributeTagInformation,
			FileTrackingInformation,
			FileIdBothDirectoryInformation,
			FileIdFullDirectoryInformation,
			FileValidDataLengthInformation,
			FileShortNameInformation,
			FileIoCompletionNotificationInformation,
			FileIoStatusBlockRangeInformation,
			FileIoPriorityHintInformation,
			FileSfioReserveInformation,
			FileSfioVolumeInformation,
			FileHardLinkInformation,
			FileProcessIdsUsingFileInformation,
			FileNormalizedNameInformation,
			FileNetworkPhysicalNameInformation,
			FileIdGlobalTxDirectoryInformation,
			FileIsRemoteDeviceInformation,
			FileUnusedInformation,
			FileNumaNodeInformation,
			FileStandardLinkInformation,
			FileRemoteProtocolInformation,
			FileRenameInformationBypassAccessCheck,
			FileLinkInformationBypassAccessCheck,
			FileVolumeNameInformation,
			FileIdInformation,
			FileIdExtdDirectoryInformation,
			FileReplaceCompletionInformation,
			FileHardLinkFullIdInformation,
			FileIdExtdBothDirectoryInformation,
			FileDispositionInformationEx,
			FileRenameInformationEx,
			FileRenameInformationExBypassAccessCheck,
			FileDesiredStorageClassInformation,
			FileStatInformation,
			FileMemoryPartitionInformation,
			FileStatLxInformation,
			FileCaseSensitiveInformation,
			FileMaximumInformation,
		}
		/// <summary>
		/// A driver sets an IRP's I/O status block to indicate the final status of an I/O request, before
		/// calling IoCompleteRequest for the IRP.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IoStatusBlock {
			[MarshalAs(UnmanagedType.U4)]
			internal NTStatus Status;
			internal ulong Unformation;
		}

		/// <summary>
		/// Flags for the <see cref="FileCaseSensitiveInformation"/> structure.
		/// </summary>
		[Flags]
		public enum CaseSensitiveFlags : uint {
			/// <summary>Specifies the directory not is case-sensitive.</summary>
			CaseInsensitiveDirectory = 0x00000000,
			/// <summary>Specifies the directory is case-sensitive.</summary>
			CaseSensitiveDirectory = 0x00000001,
		}

		/// <summary>
		/// The <see cref="FileCaseSensitiveInformation"/> structure is used to query or set per-directory
		/// case-sensitive information.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct FileCaseSensitiveInformation {
			/// <summary>Specifies if the directory is case-sensitive.</summary>
			[MarshalAs(UnmanagedType.U4)]
			public CaseSensitiveFlags Flags;
		}

		/// <summary>
		/// The NtQueryInformationFile routine returns various kinds of information about a file object.
		/// </summary>
		/// <param name="FileHandle">
		/// Handle to the file object. This handle is created by a successful call to NtCreateFile or
		/// NtOpenFile.
		/// </param>
		/// <param name="IoStatusBlock">
		/// Pointer to an IO_STATUS_BLOCK structure that receives the final completion status and
		/// information about the operation. The Information member receives the number of bytes that this
		/// routine actually writes to the FileInformation buffer.
		/// </param>
		/// <param name="FileInformation">
		/// Pointer to a caller-allocated buffer into which the routine writes the requested information
		/// about the file object. The FileInformationClass parameter specifies the type of information that
		/// the caller requests.
		/// </param>
		/// <param name="Length">The size, in bytes, of the buffer pointed to by FileInformation.</param>
		/// <param name="FileInformationClass">
		/// Specifies the type of information to be returned about the file, in the buffer that
		/// FileInformation points to. Device and intermediate drivers can specify any of the following
		/// <see cref="FileInformationClass"/> values.
		/// </param>
		/// <returns><see cref="NTStatus.Success"/> on success.</returns>
		[DllImport("ntdll.dll")]
		[return:MarshalAs(UnmanagedType.U4)]
		public static extern NTStatus NtQueryInformationFile(
			IntPtr FileHandle,
			ref IoStatusBlock IoStatusBlock,
			ref FileCaseSensitiveInformation FileInformation,
			int Length,
			FileInformationClass FileInformationClass);

		/// <summary>
		/// The NtSetInformationFile routine changes various kinds of information about a file object.
		/// </summary>
		/// <param name="FileHandle">
		/// Handle to the file object. This handle is created by a successful call to NtCreateFile or
		/// NtOpenFile.
		/// </param>
		/// <param name="IoStatusBlock">
		/// Pointer to an IO_STATUS_BLOCK structure that receives the final completion status and
		/// information about the operation. The Information member receives the number of bytes that this
		/// routine actually writes to the FileInformation buffer.
		/// </param>
		/// <param name="FileInformation">
		/// Pointer to a buffer that contains the information to set for the file. The particular structure
		/// in this buffer is determined by the FileInformationClass parameter. Setting any member of the
		/// structure to zero tells NtSetInformationFile to leave the current information about the file for
		/// that member unchanged.
		/// </param>
		/// <param name="Length">The size, in bytes, of the buffer pointed to by FileInformation.</param>
		/// <param name="FileInformationClass">
		/// The type of information, supplied in the buffer pointed to by FileInformation, to set for the
		/// file. Device and intermediate drivers can specify any of the following <see cref=
		/// "FileInformationClass"/> values.
		/// </param>
		/// <returns><see cref="NTStatus.Success"/> on success.</returns>
		[DllImport("ntdll.dll")]
		[return: MarshalAs(UnmanagedType.U4)]
		public static extern NTStatus NtSetInformationFile(
			IntPtr FileHandle,
			ref IoStatusBlock IoStatusBlock,
			ref FileCaseSensitiveInformation FileInformation,
			int Length,
			FileInformationClass FileInformationClass);
		
		/// <summary>
		/// Creates or opens a file or I/O device. The most commonly used I/O devices are as follows: file,
		/// file stream, directory, physical disk, volume, console buffer, tape drive, communications
		/// resource, mailslot, and pipe. The function returns a handle that can be used to access the file
		/// or device for various types of I/O depending on the file or device and the flags and attributes
		/// specified.
		/// </summary>
		/// 
		/// <param name="filename">
		/// The name of the file or device to be created or opened. You may use either forward slashes (/) or
		/// backslashes () in this name.
		/// </param>
		/// <param name="access">
		/// The requested access to the file or device, which can be summarized as read, write, both or
		/// neither zero).
		/// </param>
		/// <param name="share">
		/// The requested sharing mode of the file or device, which can be read, write, both, delete, all of
		/// these, or none.
		/// </param>
		/// <param name="securityAttributes">
		/// A pointer to a SECURITY_ATTRIBUTES structure that contains two separate but related data members:
		/// an optional security descriptor, and a Boolean value that determines whether the returned handle
		/// can be inherited by child processes.
		/// <para/>
		/// This parameter can be NULL.
		/// </param>
		/// <param name="creationDisposition">
		/// An action to take on a file or device that exists or does not exist.
		/// </param>
		/// <param name="flagsAndAttributes">
		/// The file or device attributes and flags, FILE_ATTRIBUTE_NORMAL being the most common default
		/// value for files.
		/// </param>
		/// <param name="templateFile">
		/// A valid handle to a template file with the GENERIC_READ access right. The template file supplies
		/// file attributes and extended attributes for the file that is being created.
		/// <para/>
		/// This parameter can be NULL.
		/// </param>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFile(
			 [MarshalAs(UnmanagedType.LPTStr)] string filename,
			 [MarshalAs(UnmanagedType.U4)] FileAccess access,
			 [MarshalAs(UnmanagedType.U4)] FileShare share,
			 IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
			 [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
			 [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
			 IntPtr templateFile);

		/// <summary>Closes an open object handle.</summary>
		/// 
		/// <param name="hObject">A valid handle to an open object.</param>
		/// <returns>True on success, otherwise false.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(
			IntPtr hObject);
		
		/// <summary>
		/// Additional <see cref="FileAttributes"/> for use with <see cref="CreateFile"/>.
		/// </summary>
		public static class CreateFileFlags {
			/// <summary>
			/// The file is being opened or created for a backup or restore operation. The system ensures that
			/// the calling process overrides file security checks when the process has SE_BACKUP_NAME and
			/// SE_RESTORE_NAME privileges. For more information, see Changing Privileges in a Token.
			/// <para/>
			/// You must set this flag to obtain a handle to a directory. A directory handle can be passed to
			/// some functions instead of a file handle. For more information, see the Remarks section.
			/// </summary>
			public const FileAttributes BackupSemantics = (FileAttributes) 0x02000000;

			/// <summary>
			/// The file is to be deleted immediately after all of its handles are closed, which includes the
			/// specified handle and any other open or duplicated handles.<para/>
			/// If there are existing open handles to a file, the call fails unless they were all opened with the
			/// FILE_SHARE_DELETE share mode.
			/// <para/>
			/// You must set this flag to obtain a handle to a directory. A directory handle can be passed to
			/// some functions instead of a file handle. For more information, see the Remarks section.
			/// </summary>
			public const FileAttributes DeleteOnClose = (FileAttributes) 0x04000000;
		}

		/// <summary>Additional flags that were not implemented for <see cref="FileAccess"/>.</summary>
		public static class FileAccessFlags {
			/// <summary>The right to write file attributes.</summary>
			public const FileAccess WriteAttributes = (FileAccess) 0x100;
		}

		/// <summary>Win32 error codes.</summary>
		public static class ErrorCodes {
			/// <summary>The system cannot find the file specified.</summary>
			public const int FileNotFound = 0x00000002;
			/// <summary>Access is denied.</summary>
			public const int AccessDenied = 0x00000005;
		}
	}
}
