#load Files.csx;

public static class Git
{
	public static string PathToExe { get; set; }
	public static string FullPathExe
	{
		get { return Files.LookForFileInFolders("git.exe", @"C:\Program Files (x86)\Git\bin",
					@"C:\Program Files\Git\bin", PathToExe ?? string.Empty); } }

	public static string CurrentSha()
	{
		var result = BuildHelper.RunTask(FullPathExe, "rev-parse HEAD", false);
		return result.Success ? result.Output : string.Empty;
	}

	public static string CurrentBranch()
	{
		var result = BuildHelper.RunTask(FullPathExe, "rev-parse --abbrev-ref HEAD", false);
		return result.Success ? result.Output : string.Empty;
	}

	public static void Tag(string tag)
	{
		BuildHelper.RunTask(FullPathExe, "tag "+ tag);
	}

	public static void ResetAllModifications()
	{
		BuildHelper.RunTask(FullPathExe, "reset --hard");
	}

	public static void Clean(bool allFiles = false)
	{
		BuildHelper.RunTask(FullPathExe, "clean -f" + (allFiles? " -dx" : string.Empty));
	}
	
	public static Result Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}