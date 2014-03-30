public static class XUnit
{
	public static string PathToExe { get; set; }
	public static string FullPathExe
	{
		get
		{
			return Files.LookForFileInFolders("xunit.console.clr4.exe",
				PathToExe ?? string.Empty);
		}
	}

	public static bool Run(string assembly, string resultFile = "TestResults.xml")
	{
		return BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, resultFile));
	}

	public static string GetParameters(string assembly, string resultFile = "TestResults.xml")
	{
		return assembly + " /xml "+ resultFile;
	}

	public static bool Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}