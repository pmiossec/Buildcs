using System.Text.RegularExpressions;

public static class VsTest
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "vstest.console.exe"); } }
	public static string ResultFile { get; private set; }

	public static bool Run(IEnumerable<string> assemblies, string testsettings = null)
	{
		var success = BuildHelper.RunTask(FullPathExe, GetParameters(assemblies, testsettings), false, true);
		ExtractResultFileFromOutput(BuildHelper.LastTaskOutput);
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

	public static string GetParameters(IEnumerable<string> assemblies, string testsettings = null)
	{
		return string.Join(" ", assemblies) + " " + BuildHelper.BuildCommand("/logger:trx");
	}

	public static bool Run(string parameters)
	{
		var success = BuildHelper.RunTask(FullPathExe, parameters, false, true);
		ExtractResultFileFromOutput(BuildHelper.LastTaskOutput);
		return success;
	}
}
