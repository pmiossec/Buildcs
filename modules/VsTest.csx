#load Files.csx;
using System.Text.RegularExpressions;

public static class VsTest
{
	private static string defaultVsPath = @"..\IDE\CommonExtensions\Microsoft\TestWindow\";
	public static string PathToExe { get; set; }
	public static string FullPathExe
	{
		get
		{
			return Files.LookForFileInFolders("vstest.console.exe",
				BuildHelper.GetEnvironnementVariable("VS120COMNTOOLS") + defaultVsPath,
				BuildHelper.GetEnvironnementVariable("VS110COMNTOOLS") + defaultVsPath,
				PathToExe ?? string.Empty);
		}
	}

	public static string ResultFile { get; private set; }
	public static string CoverageResultFile { get; private set; }

	public static Result Run(IEnumerable<string> assemblies, string testsettings = null, bool enableCodeCoverage = false)
	{
		var result = BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, testsettings, enableCodeCoverage), true);
		ExtractResultFileFromOutput(result.Output);
		if(enableCodeCoverage)
			ExtractCoverResultFileFromOutput(result.Output);
		return result;
	}

	public static string ExtractResultFileFromOutput(string output)
	{
		var fileRegex = new Regex(@"^Results File:(?<file>.*)$", RegexOptions.Multiline);
		var result = fileRegex.Match(output);
		if (result.Success)
		{
			ResultFile = result.Groups["file"].Value.Trim();
			BuildHelper.DisplayAndLog("Found test result file:" + ResultFile);
			return ResultFile;
		}

		return null;
	}

	public static string ExtractCoverResultFileFromOutput(string output)
	{
		var fileRegex = new Regex(@"Attachments:(?<file>.*\.coverage)", RegexOptions.Singleline);
		var result = fileRegex.Match(output);
		if (result.Success)
		{
			CoverageResultFile = result.Groups["file"].Value.Trim();
			BuildHelper.DisplayAndLog("Found coverage result file:" + CoverageResultFile);
			return ResultFile;
		}

		return null;
	}

	public static string GetParameters(IEnumerable<string> assemblies, string testsettings = null, bool enableCodeCoverage = false)
	{
		var assembliesParams = string.Join(" ", assemblies);
		return BuildHelper.BuildCommand( assembliesParams, "/logger:trx",
			(enableCodeCoverage ? "/EnableCodeCoverage" : string.Empty),
			(testsettings == null ? string.Empty : ("/Settings:" + testsettings)));
	}

	public static Result Run(string parameters)
	{
		var result = BuildHelper.RunTask(FullPathExe, parameters, true);
		ExtractResultFileFromOutput(result.Output);
		return result;
	}
}
