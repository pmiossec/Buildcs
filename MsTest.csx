public static class MsTest
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "MSTest.exe"); } }

	public static void Run(IEnumerable<string> assemblies, string resultFile = "mstest.trx", string testsettings = null)
	{
		Build.RunTask(FullPathExe, GetParameters(assemblies, resultFile, testsettings));
	}

	public static string GetParameters(IEnumerable<string> assemblies, string resultFile = "mstest.trx", string testsettings = null)
	{
		string paramAssemblies = string.Empty;
		foreach(var assembly in assemblies)
		{
			paramAssemblies += @" /testcontainer:" + assembly;
		}

		return Build.BuildCommand(paramAssemblies, "/resultsfile:"+ resultFile, testsettings == null ? string.Empty : "/testSettings:" + testsettings , "/nologo");
	}

	public static void Run(string parameters)
	{
		Build.RunTask(FullPathExe, parameters);
	}
}