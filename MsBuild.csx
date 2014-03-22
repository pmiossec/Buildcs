public static class MsBuild
{
	public static string MsBuildPath { get; set; }
	public static string MsBuildFullPath { get { return Path.Combine(MsBuildPath ?? string.Empty, "msbuild.exe"); } }

	public static void Run(string projectFile, string configuration = "Release", bool parallelBuild = true, string target = "build", string otherParameters = null)
	{
		var buildParams = BuildCommand(projectFile, "/t:" + target , parallelBuild ? "/m" : string.Empty, " /p:Configuration=" + configuration, otherParameters);
		Build.RunTask(MsBuildFullPath, buildParams);
	}

	public static void Clean(string projectFile, string configuration = "Release")
	{
		Build.RunTask(MsBuildFullPath, BuildCommand(projectFile, "/t:clean", "/p:Configuration=" + configuration));
	}

	public static void Run(string parameters)
	{
		Build.RunTask(MsBuildFullPath, parameters);
	}

	private static string BuildCommand(params string[] parameters)
	{
		return string.Join(" ", parameters);
	}
}