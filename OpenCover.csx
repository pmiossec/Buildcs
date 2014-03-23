public static class OpenCover
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "OpenCover.Console.exe"); } }

	public static bool Run(string assemblyFilter, string target, string targetargs, string coverageReportFile = "coverage-report.trx")
	{
		return Build.RunTask(FullPathExe, Build.BuildCommand("-register:user", "-filter:" + assemblyFilter, "-target:\"" + target + "\"", "-targetargs:\"" + targetargs + "\"", "-output:" + coverageReportFile), true);
	}

	public static void Run(string parameters)
	{
		Build.RunTask(FullPathExe, parameters);
	}
}