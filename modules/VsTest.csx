using System.Text.RegularExpressions;

public static class VsTest
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "vstest.console.exe"); } }
	public static string ResultFile { get; private set; }

	public static bool Run(IEnumerable<string> assemblies, string testsettings = null)
	{
		string output;
		var success = BuildHelper.RunTask(out output, FullPathExe, GetParameters(assemblies, testsettings));
		ExtractResultFileFromOutput(output);
		return success;
	}

	private static void ExtractResultFileFromOutput(string output)
	{
		var fileRegex = new Regex(@"^Results File:(?<file>.*)$", RegexOptions.Multiline);
		var result = fileRegex.Match(output);
		if (result.Success)
		{
			ResultFile = result.Groups["file"].Value.Trim();
		}
	}
	
	public static string GetParameters(IEnumerable<string> assemblies, string testsettings = null)
	{
		return string.Join(" ", assemblies) + " " + BuildHelper.BuildCommand("/logger:trx");
	}

	public static bool Run(string parameters)
	{
		string output;
		var success = BuildHelper.RunTask(out output, FullPathExe, parameters);
		ExtractResultFileFromOutput(output);
		return success;
	}
}