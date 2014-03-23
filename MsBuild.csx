public static class MsBuild
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "msbuild.exe"); } }

	public static void Run(string projectFile, string configuration = "Release", bool parallelBuild = true, string target = "build", string otherParameters = null)
	{
		var buildParams = Build.BuildCommand(projectFile, "/t:" + target , parallelBuild ? "/m" : string.Empty, " /p:Configuration=" + configuration, otherParameters);
		Build.RunTask(FullPathExe, buildParams);
	}

	public static void Clean(string projectFile, string configuration = "Release")
	{
		Build.RunTask(FullPathExe, Build.BuildCommand(projectFile, "/t:clean", "/p:Configuration=" + configuration));
	}

	public static void Run(string parameters)
	{
		Build.RunTask(FullPathExe, parameters);
	}
}