public static class ReportGenerator
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Files.LookForFileInFolders("reportgenerator.exe", PathToExe ?? string.Empty); } }

	public static Result Run(string coverageReportFile, string coverageReportFolder = "coverage_report")
	{
		return BuildHelper.RunTask(FullPathExe, BuildHelper.BuildCommand("\"-reports:" + coverageReportFile + "\"", "\"-targetdir:" + coverageReportFolder + "\""));
	}

	public static Result Run(string parameters)
	{
		return BuildHelper.RunTask(FullPathExe, parameters);
	}
}