public static class MsBuild
{
	public static void Run(string projectFile, string configuration = "Release", bool parallelBuild = true, string otherParameters = null)
	{
		string buildParams = projectFile;
		if(parallelBuild)
			buildParams += " /m";
		buildParams += " /p:Configuration=" + configuration;
		buildParams += " " + otherParameters;

		Build.RunTask("msbuild.exe", buildParams);
	}
}