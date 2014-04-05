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

	public static bool Run(IEnumerable<string> assemblies, string testsettings = null, bool enableCodeCoverage = false)
	{
		var success = BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, testsettings, enableCodeCoverage), true);
		ExtractResultFileFromOutput(BuildHelper.LastTaskOutput);
		if(enableCodeCoverage)
			ExtractCoverResultFileFromOutput(BuildHelper.LastTaskOutput);
		return success;
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
		return string.Join(" ", assemblies) + " " + BuildHelper.BuildCommand("/logger:trx") + (enableCodeCoverage? " /EnableCodeCoverage" : string.Empty);
	}

	public static bool Run(string parameters)
	{
		var success = BuildHelper.RunTask(FullPathExe, parameters, true);
		ExtractResultFileFromOutput(BuildHelper.LastTaskOutput);
		return success;
	}
}
