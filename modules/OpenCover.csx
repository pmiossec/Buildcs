public static partial class OpenCover
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Files.LookForFileInFolders("OpenCover.Console.exe", PathToExe ?? string.Empty); } }

	public static Result Run(string assemblyFilter, string target, string targetargs, string coverageReportFile = "coverage_report.trx")
	{
		if(Path.GetDirectoryName(target) == string.Empty)
			BuildHelper.DisplayAndLog("OpenCover=>warning: target parameter MUST be an absolute path toward the test framework executable", DisplayLevel.Warning);
		return BuildHelper.RunTask(FullPathExe, BuildHelper.BuildCommand("-register:user", "-filter:" + assemblyFilter, "-target:\"" + target + "\"", "-targetargs:\"" + targetargs + "\"", "-output:" + coverageReportFile));
	}

	public static Result Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}