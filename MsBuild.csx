public static class MsBuild
{
	public static void Run(string projectFile, bool parallelBuild = true, string otherParameters = null)
	{
		string buildParams = projectFile;
		if(parallelBuild)
			buildParams += " /m";
		buildParams += " " + otherParameters;

		Build.RunTask("msbuild.exe", buildParams);
	}
}