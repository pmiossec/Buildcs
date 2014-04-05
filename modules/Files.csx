using System.Text.RegularExpressions;

public class Files
{
	//Copy folder content to another folder
	public static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwrite = false)
	{
		BuildHelper.DisplayAndLog("Copying directory '" + sourceDirName + "' to '" + destDirName + "'...");
		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = dir.GetDirectories();

		if (!dir.Exists)
		{
			if(BuildHelper.ContinueOnError)
			{
				BuildHelper.DisplayAndLog("Source directory does not exist or could not be found: " + sourceDirName, DisplayLevel.Error);
				BuildHelper.DisplayAndLog("Continue anyway...", DisplayLevel.Warning);
				return;
			}

			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}

		// If the destination directory doesn't exist, create it. 
        if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		// Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, overwrite);
		}

		// If copying subdirectories, copy them and their contents to new location. 
		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				CopyFolder(subdir.FullName, temppath, copySubDirs, overwrite);
			}
		}
	}

	//Copy a file
	public static void CopyFile(string sourceName, string destName, bool overwrite = true)
	{
		if(!System.IO.File.Exists(sourceName))
		{
			BuildHelper.DisplayAndLog("warning: the file to copy '" + sourceName + "' doesn't exist!", DisplayLevel.Warning);
			return;
		}

		BuildHelper.ContinueOrFail(() => { File.Copy(sourceName, destName, overwrite); });
	}

	//Delete a file
	public static void DeleteFile(string filePath)
	{
		if(!System.IO.File.Exists(filePath))
			return;
		BuildHelper.ContinueOrFail(
			() => {
			BuildHelper.DisplayAndLog("Deleting file '" + filePath + "'...");
			System.IO.File.Delete(filePath);
		});
	}

	//Get all the files of a directory following a regex patern
	public static string[] GetFilesWithPattern(string parentDirectoryPath, string filePattern, bool subdirectories = false)
	{
		if(!System.IO.Directory.Exists(parentDirectoryPath))
			return new string[0];

		return BuildHelper.ContinueOrFail(() => {
			return System.IO.Directory.GetFiles(parentDirectoryPath, filePattern, subdirectories ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
		});
	}

	//Delete all the files of a directory following a regex patern
	public static void DeleteFilesWithPattern(string parentDirectoryPath, string filePattern)
	{
		if(!System.IO.Directory.Exists(parentDirectoryPath))
			return;

		BuildHelper.ContinueOrFail(() => {
			foreach(var directory in GetFilesWithPattern(parentDirectoryPath, filePattern))
				DeleteFile(directory);
			});
	}

	//Delete all the subdirectories of a directory following a regex patern
	public static void DeleteDirectoriesWithPattern(string parentDirectoryPath, string directoryPattern)
	{
		if(!System.IO.Directory.Exists(parentDirectoryPath))
			return;

		BuildHelper.ContinueOrFail(() => {
			foreach(var directory in System.IO.Directory.GetDirectories(parentDirectoryPath, directoryPattern))
				DeleteDirectory(directory);
			});
	}

	//Delete a directory
	public static void DeleteDirectory(string directoryPath)
	{
		if(!System.IO.Directory.Exists(directoryPath))
			return;

		BuildHelper.DisplayAndLog("Deleting directory '" + directoryPath + "'...");
		BuildHelper.ContinueOrFail(() => {
			System.IO.Directory.Delete(directoryPath, true);
			});
	}

	public static void ReplaceText(string filePath, string regex, string newText)
	{
		if(!System.IO.File.Exists(filePath))
			return;

		BuildHelper.DisplayAndLog("Replacing text in file '" + filePath + "'...");
		BuildHelper.ContinueOrFail(() => {
			File.WriteAllText(filePath, Regex.Replace(File.ReadAllText(filePath), regex, newText));
			});
	}

	//Look for a file in different folders and return the full path where it is found
	public static string LookForFileInFolders(string filename, params string[] folders)
	{
		foreach(var folder in folders)
		{
			var path = Path.Combine(folder, filename);
			if(File.Exists(path))
				return path;
		}
		return filename;
	}
}
