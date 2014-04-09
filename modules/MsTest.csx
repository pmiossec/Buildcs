public static partial class MsTest
{
	private static string defaultVsPath = @"..\IDE\";
	public static string PathToExe { get; set; }
	public static string FullPathExe
	{
		get
		{
			return Files.LookForFileInFolders("MSTest.exe",
				BuildHelper.GetEnvironnementVariable("VS120COMNTOOLS") + defaultVsPath,
				BuildHelper.GetEnvironnementVariable("VS110COMNTOOLS") + defaultVsPath,
				BuildHelper.GetEnvironnementVariable("VS100COMNTOOLS") + defaultVsPath,
				BuildHelper.GetEnvironnementVariable("VS90COMNTOOLS") + defaultVsPath,
				PathToExe ?? string.Empty);
		}
	}

	public static Result Run(IEnumerable<string> assemblies, string resultFile = "TestResults.trx", string testsettings = null)
	{
		return BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, resultFile, testsettings));
	}

	public static string GetParameters(IEnumerable<string> assemblies, string resultFile = "TestResults.trx", string testsettings = null)
	{
		string paramAssemblies = string.Empty;
		foreach(var assembly in assemblies)
		{
			paramAssemblies += @" /testcontainer:" + assembly;
		}

		return BuildHelper.BuildCommand(paramAssemblies, "/resultsfile:"+ resultFile, testsettings == null ? string.Empty : "/testSettings:" + testsettings , "/nologo");
	}

	public static Result Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}