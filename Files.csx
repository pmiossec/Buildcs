using System.Text.RegularExpressions;

public class Files
{
	//Copy folder content to another folder
    public static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwrite = false)
	{
		// Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = dir.GetDirectories();

		if (!dir.Exists)
		{
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

	//Delete a file
	public static void DeleteFile(string filePath)
	{
		if(System.IO.File.Exists(filePath))
		{
			System.IO.File.Delete(filePath);
		}
	}

	//Delete all the files of a directory following a regex patern
	public static void DeleteFilesWithPattern(string parentDirectoryPath, string filePattern)
	{
		foreach(var directory in System.IO.Directory.GetFiles(parentDirectoryPath, filePattern))
		{
			DeleteFile(directory);
		}
	}

	//Delete all the subdirectories of a directory following a regex patern
	public static void DeleteDirectoriesWithPattern(string parentDirectoryPath, string directoryPattern)
	{
		foreach(var directory in System.IO.Directory.GetDirectories(parentDirectoryPath, directoryPattern))
		{
			DeleteDirectory(directory);
		}
	}

	//Delete a directory
	public static void DeleteDirectory(string directoryPath)
	{
		if(System.IO.Directory.Exists(directoryPath))
		{
			System.IO.Directory.Delete(directoryPath, true);
		}
	}

	public static void ReplaceText(string filePath, string regex, string newText)
	{
		File.WriteAllText(filePath, Regex.Replace(File.ReadAllText(filePath), regex, newText));
	}
}
