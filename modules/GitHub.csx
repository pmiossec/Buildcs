#r OctokitWrapper.dll

using OctokitWrapper;

public static partial class GitHub
{
	public static bool Release()
	{
		var github = new OctokitWrapper();
		github.Repository = 
		github.OauthToken { get; set; }
		github.TagName { get; set; }

		var result = BuildHelper.RunTask(FullPathExe, "rev-parse HEAD", false);
		return result.Success ? result.Output : string.Empty;
	}
}
