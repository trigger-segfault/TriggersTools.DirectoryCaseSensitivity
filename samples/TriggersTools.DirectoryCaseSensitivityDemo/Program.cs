using System;
using System.IO;

namespace TriggersTools.IO.Windows.Demo {
	class Program {
		static void Main(string[] args) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Does this version of Windows support directory case-sensitivity?");
			Console.WriteLine($"    {(DirectoryCaseSensitivity.IsSupported() ? "Yes" : "No")}");
			Console.WriteLine();
			Console.ResetColor();

			// Why are you running this demo on < Windows 10?
			if (!DirectoryCaseSensitivity.IsSupported()) {
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine("Not supported, ending demo.");
				Console.ResetColor();
				Console.WriteLine("Press any key to continue...");
				Console.Read();
			}

			string baseDir = Directory.GetCurrentDirectory();

			string dir1 = Path.Combine(baseDir, "CaseSensitivityTest");
			string dir2A = Path.Combine(dir1, "SubdirA");
			string dir2B = Path.Combine(dir1, "SubdirB");
			string dir2C = Path.Combine(dir1, "SubdirC");
			string file1A = Path.Combine(dir1, "file1.txt");
			string file1B = Path.Combine(dir1, "FILE1.txt");
			string file2 = Path.Combine(dir1, "file2.txt");

			WriteHeader("Initial Setup");
			CreateDirectory(dir1);
			if (!CheckCS(dir1))
				SetCS(dir1, true);
			Console.WriteLine();

			WriteHeader("Create case-sensitive files");
			CreateFile(file1A);
			CreateFile(file1B);
			CreateFile(file2);
			ListFiles(dir1);
			Console.WriteLine();

			WriteHeader("Disable case-sensitivity of directory with matchig files");
			// Fails because directory is not empty
			SetCS(dir1, false);
			Console.WriteLine();

			WriteHeader("Retry after clearing files");
			// Clear files and set again
			DeleteFile(file1B);
			//ClearDirector(dir1);
			SetCS(dir1, false);
			Console.WriteLine();

			CreateFile(file1A);
			CreateFile(file1B);
			ListFiles(dir1);
			Console.WriteLine();

			WriteHeader("Test new subdirectory case-sensitivity");
			SetCS(dir1, true);
			CreateDirectory(dir2A);
			CheckCS(dir1);
			CheckCS(dir2A);
			Console.WriteLine();
			WriteHeader("Directories do not inherit case sensitivity by default:\n" +
				"Use DirectoryCaseSensitivity's Create(), Inherit(), and CreateInherit()");

			//SetCS(dir1, false);
			Inherit(dir2A);
			CreateDirectory(dir2B, true);
			CreateDirectoryInherit(dir2C);
			CheckCS(dir1);
			CheckCS(dir2A);
			CheckCS(dir2B);
			CheckCS(dir2C);
			Console.WriteLine();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}

		static void WriteStatus(string label, string file, string extra = null) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write($"{label}: ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			if (extra != null) {
				Console.Write(Path.GetFileName(file));
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($" {extra}");
			}
			else {
				Console.WriteLine(Path.GetFileName(file));
			}
			Console.ResetColor();
		}

		static void WriteHeader(string message) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{message}:");
			Console.ResetColor();
		}

		static void WriteError(string message) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		static void ClearDirector(string directory) {
			foreach (string file in Directory.EnumerateFiles(directory)) {
				File.Delete(file);
			}
			foreach (string subdir in Directory.EnumerateDirectories(directory)) {
				foreach (string file in Directory.EnumerateFiles(subdir)) {
					File.Delete(file);
				}
				Directory.Delete(subdir);
			}
			WriteStatus("Directory Cleared", directory);
		}

		static void ListFiles(string directory) {
			Console.ForegroundColor =  ConsoleColor.White;
			Console.WriteLine($" + {Path.GetFileName(directory)}");
			foreach (string file in Directory.EnumerateFileSystemEntries(directory)) {
				Console.WriteLine($" |-- {Path.GetFileName(file)}");
			}
			Console.ResetColor();
		}

		static void CreateDirectory(string directory) {
			if (!Directory.Exists(directory)) {
				Directory.CreateDirectory(directory);
				WriteStatus("Directory Created", directory);
			}
			else {
				WriteStatus("Directory Exists", directory);
				ClearDirector(directory);
			}
		}

		static void CreateDirectoryInherit(string directory) {
			if (!Directory.Exists(directory)) {
				bool enabled = DirectoryCaseSensitivity.CreateInherit(directory);
				WriteStatus("Directory Inherit Created", directory, (enabled ? "enabled" : "disabled"));
			}
			else {
				WriteStatus("Directory Exists", directory);
				ClearDirector(directory);
			}
		}

		static void Inherit(string directory) {
			if (Directory.Exists(directory)) {
				bool enabled = DirectoryCaseSensitivity.Inherit(directory);
				WriteStatus("Directory Inherited", directory, (enabled ? "enabled" : "disabled"));
			}
		}

		static void CreateDirectory(string directory, bool caseSensitive) {
			if (!Directory.Exists(directory)) {
				DirectoryCaseSensitivity.Create(directory, caseSensitive);
				WriteStatus("Directory Created", directory, (caseSensitive ? "enabled" : "disabled"));
			}
			else {
				WriteStatus("Directory Exists", directory);
				ClearDirector(directory);
			}
		}

		static void DeleteDirectory(string directory) {
			if (Directory.Exists(directory)) {
				ClearDirector(directory);
				Directory.Delete(directory);
				WriteStatus("Directory Deleted", directory);
			}
			else {
				WriteStatus("Directory Does not Exist", directory);
			}
		}

		static void CreateFile(string file) {
			if (!File.Exists(file)) {
				File.Create(file).Close();
				WriteStatus("File Created", file);
			}
			else {
				WriteStatus("File Exists", file);
			}
		}

		static void DeleteFile(string file) {
			if (File.Exists(file)) {
				File.Delete(file);
				WriteStatus("File Deleted", file);
			}
			else {
				WriteStatus("File Does not Exist", file);
			}
		}

		static bool CheckCS(string directory) {
			bool enabled = DirectoryCaseSensitivity.IsCaseSensitive(directory);
			WriteStatus("Check Case Sensitivity", directory, (enabled ? "enabled" : "disabled"));
			return enabled;
		}

		static void SetCS(string directory, bool enabled) {
			try {
				DirectoryCaseSensitivity.SetCaseSensitive(directory, enabled);
				WriteStatus("Set Case Sensitivity", directory, (enabled ? "enabled" : "disabled"));
			}
			catch (IOException ex) {
				WriteError(ex.Message);
			}
		}
	}
}
