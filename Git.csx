public static class Git
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "git.exe"); } }

	public static string CurrentSha()
	{
		string sha1;
		BuildHelper.RunTask(out sha1, FullPathExe, "rev-parse HEAD", false);
		return sha1;
	}

	public static string CurrentBranch()
	{
		string branch;
		BuildHelper.RunTask(out branch, FullPathExe, "rev-parse --abbrev-ref HEAD", false);
		return branch;
	}
	
	public static void Run(string parameters)
	{
		BuildHelper.RunTask(FullPathExe, parameters);
	}
}