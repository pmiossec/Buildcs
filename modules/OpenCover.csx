public static class OpenCover
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "OpenCover.Console.exe"); } }

	public static bool Run(string assemblyFilter, string target, string targetargs, string coverageReportFile = "coverage_report.trx")
	{
		if(Path.GetDirectoryName(target) == string.Empty)
			BuildHelper.DisplayAndLog("OpenCover=>warning: target parameter MUST be an absolute path toward the test framework executable", DisplayLevel.Warning);
		return BuildHelper.RunTask(FullPathExe, BuildHelper.BuildCommand("-register:user", "-filter:" + assemblyFilter, "-target:\"" + target + "\"", "-targetargs:\"" + targetargs + "\"", "-output:" + coverageReportFile), true);
	}

	public static bool Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}