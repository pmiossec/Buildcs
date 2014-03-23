public static class MsBuild
{
	public static string PathToExe { get; set; }
	public static string FullPathExe { get { return Path.Combine(PathToExe ?? string.Empty, "msbuild.exe"); } }

	public static void Run(string projectFile, string configuration = "Release", bool parallelBuild = true, string target = "Build", string otherProperties = null, string otherParameters = null)
	{
		var buildParams = BuildHelper.BuildCommand(projectFile, "/t:" + target , parallelBuild ? "/m" : string.Empty, " /p:Configuration=" + configuration + otherProperties, otherParameters);
		BuildHelper.RunTask(FullPathExe, buildParams);
	}

	public static void Clean(string projectFile, string configuration = "Release")
	{
		BuildHelper.RunTask(FullPathExe, BuildHelper.BuildCommand(projectFile, "/t:clean", "/p:Configuration=" + configuration));
	}

	public static void Run(string parameters)
	{
		BuildHelper.RunTask(FullPathExe, parameters);
	}
}