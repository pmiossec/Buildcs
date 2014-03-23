public static class VsTest
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "vstest.console.exe"); } }

	public static void Run(IEnumerable<string> assemblies, string resultFile = "vstest.trx", string testsettings = null)
	{
		BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, resultFile, testsettings));
	}

	public static string GetParameters(IEnumerable<string> assemblies, string resultFile = "vstest.trx", string testsettings = null)
	{
		return string.Join(" ", assemblies) + " " + BuildHelper.BuildCommand("/logger:trx");
	}

	public static void Run(string parameters)
	{
		BuildHelper.RunTask(FullPathExe, parameters);
	}
}