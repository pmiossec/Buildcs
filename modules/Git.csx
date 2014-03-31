public static class Git
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "git.exe"); } }

	public static string CurrentSha()
	{
		BuildHelper.RunTask(FullPathExe, "rev-parse HEAD", false, false);
		return BuildHelper.LastTaskOutput;
	}

	public static string CurrentBranch()
	{
		BuildHelper.RunTask(FullPathExe, "rev-parse --abbrev-ref HEAD", false, false);
		return BuildHelper.LastTaskOutput;
	}

	public static void Tag(string tag)
	{
		BuildHelper.RunTask(FullPathExe, "tag "+ tag);
	}
	
	public static bool Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}