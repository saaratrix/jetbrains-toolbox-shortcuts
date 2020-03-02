using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace JetbrainsToolboxShortcutsGenerator
{
	public class Program
	{
		// Default folder for unconfigured Jetbrains Toolbox most likely here: C:\Users\User\AppData\Local\JetBrains\Toolbox\Apps\...
		public static string ToolboxFolder = Environment.CurrentDirectory;
		public static string LinkFolder = Environment.CurrentDirectory;
		public static int MaxDepth = 10;

		public static string BinariesFile = "jetbrainsbinaries.txt";
		/// <summary>
		/// File inside the ch-0 folder where the folder versions are.
		/// If this file exists we can assume the folders are the versions excluding version.plugins folders but they don't have the binaries in them.
		/// </summary>
		public static string BuildFile = "build.txt";
		/// <summary>
		/// The Jetbrains exe files we're looking for.
		/// </summary>
		private static List<string> Binaries = new List<string>
		{
				"datagrip64.exe", "pycharm64.exe", "rider64.exe", "webstorm64.exe", 
				// As I don't have these installed I am not sure of the binary paths:
				"clion64.exe", "goland64.exe", "idea64.exe", "phpstorm64.exe", "rubymine64.exe", 
		};
		public static int ShortcutsGenerated = 0;
		public static Dictionary<string, List<BinarySearchResult>> SearchResults = new Dictionary<string, List<BinarySearchResult>>();
		
		static void Main(string[] args)
		{
			ParseArgs(args);

			if (!Directory.Exists(ToolboxFolder))
			{
				Console.WriteLine($"Jetbrains Toolbox folder was not found: {ToolboxFolder}");
				return;
			}
			if (!Directory.Exists(LinkFolder))
			{
				Console.WriteLine($"Shortcuts folder was not found: {LinkFolder}");
				return;
			}
			
			ParseBinaries();
			
			FindLinks(ToolboxFolder, 0, "");
			GenerateLinks();
			
			
			if (ShortcutsGenerated == 0)
				Console.WriteLine("Did not find any binaries to create shortcuts for.");
			else
				Console.WriteLine($"Generated {ShortcutsGenerated} shortcuts.");
		}

		public static void ParseArgs(string[] args)
		{
			if (args.Length < 2)
				return;

			for (int i = 0; i < args.Length; i += 2)
			{
				int pathIndex = i + 1;
				// Make sure that the actual path exists too.
				if (pathIndex >= args.Length)
					break;
				
				string arg = args[i];
				switch (arg)
				{
					case "-t":
						ToolboxFolder = args[pathIndex];
						break;
					case "-l":
						LinkFolder = args[pathIndex];
						break;
				}
			}
		}

		/// <summary>
		/// Looks for a file "jetbrainsbinaries.txt" to parse the name of the binaries.
		/// </summary>
		public static void ParseBinaries()
		{
			string file = Path.Combine(Environment.CurrentDirectory, BinariesFile);
			if (!File.Exists(file))
				return;

			Binaries = new List<string>();
			
			using (StreamReader sr = new StreamReader(file))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (string.IsNullOrWhiteSpace(line))
						continue;
					
					Binaries.Add(line.Trim().ToLower());
				}
			}
		}

		/// <summary>
		/// Recursively look through all the files & directories.
		/// </summary>
		public static void FindLinks(string path, int depth, string version)
		{
			var files = Directory.GetFiles(path, "*.exe");
			foreach (var file in files)
			{
				var filename = Path.GetFileName(file);
				if (!Binaries.Contains(filename.ToLower()))
					continue;

				CreateSearchResult(file, filename, version);
			}

			string historyFilePath = Path.Combine(path, BuildFile);
			if (File.Exists(historyFilePath))
			{
				version = GetVersionFromPath(path);
			}

			if (depth >= MaxDepth)
				return;

			var directories = Directory.GetDirectories(path);
			foreach (var directory in directories)
			{
				FindLinks(directory, depth + 1, version);
			}
		}

		/// <summary>
		/// Example path: C:\Programming\Jetbrains\apps\WebStorm\ch-0\193.6494.34
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetVersionFromPath(string path)
		{
			string version = path.Split(Path.DirectorySeparatorChar).Last();
			return version;
		}

		public static void CreateSearchResult(string filepath, string filename, string version)
		{
			if (string.IsNullOrWhiteSpace(version))
				Console.WriteLine($"Couldn't detect version for {filepath}\\{filename}");
			
			if (!SearchResults.ContainsKey(filename))
				SearchResults[filename] = new List<BinarySearchResult>();
			
			SearchResults[filename].Add(new BinarySearchResult()
			{
					Filename = filename,
					Filepath = filepath,
					Version = version
			});
		}
		
		private static void GenerateLinks()
		{
			foreach (var kvp in SearchResults)
			{
				var searchResult = GetHighestVersionResult(kvp.Value);
				if (searchResult == null)
					continue;
				
				GenerateShortcut(searchResult.Filepath, searchResult.Filename);
			}
		}

		public static BinarySearchResult GetHighestVersionResult(List<BinarySearchResult> searchResults)
		{
			var sortedList = searchResults.OrderByDescending(v => v.Version);
			return sortedList.FirstOrDefault();
		}

		// Source for all the IShellLink stuff: https://stackoverflow.com/a/14632782/2437350
		// Modified to fit this app.
		public static void GenerateShortcut(string filepath, string filename)
		{
			// This creates for example "pycharm64.exe - Shortcut.lnk" 
			// But only pycharm64.exe - Shortcut is visible in explorer.
			string linkFilename = $"{filename} - Shortcut.lnk";
			string linkPath = Path.Combine(LinkFolder, linkFilename);

			IShellLink link = (IShellLink)new ShellLink();

			link.SetPath(filepath);
			// save it
			var file        = (IPersistFile)link;
			file.Save(linkPath, false);
			Console.WriteLine($"Generated shortcut for file: {filename} at {linkPath} targeting {filepath}");
			ShortcutsGenerated++;
		}
	}

	public class BinarySearchResult
	{
		public string Filename { get; set; }
		public string Filepath { get; set; }
		public string Version { get; set; }
	}
	
	
	[ComImport]
	[Guid("00021401-0000-0000-C000-000000000046")]
	internal class ShellLink
	{
	}
	
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	internal interface IShellLink
	{
		void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
		void GetIDList(out IntPtr ppidl);
		void SetIDList(IntPtr pidl);
		void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
		void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
		void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
		void GetHotkey(out short pwHotkey);
		void SetHotkey(short wHotkey);
		void GetShowCmd(out int piShowCmd);
		void SetShowCmd(int iShowCmd);
		void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
		void Resolve(IntPtr hwnd, int fFlags);
		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}
}