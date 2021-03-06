#r .\modules\GitHubWrapper.dll
#load Build.csx
#load Release.perso.csx

using GitHubWrapper;
//Usage: scriptcs.exe BuildHelper.csx -- /v:1.0

//Need to create a partial class containing your personal datas (that must not been commited!) like that:
//public partial class ReleaseBuild
//{
//	string GitHubAuthToken = "C4BD2DF21A8BAC6FF6537C54B9697402";
//	string GitHubLogin = "myAccount";
//}

BuildHelper.RunTarget(typeof(ReleaseBuild), Env.ScriptArgs);

public partial class ReleaseBuild : BuildHelper
{
	public ReleaseBuild() { Clean(keepResultFiles:false); }

	~ReleaseBuild() { BuildHelper.ContinueOnError = true; Clean(keepResultFiles:true); }

	[Target]
	void Release()
	{
		var softwareName = "buildcs";
		var version = BuildHelper.GetArguments("/v:", string.Empty);
		if(string.IsNullOrEmpty(version))
			throw new Exception("You must specify a version number! (/v:X.Y.Z)");
		if(string.IsNullOrEmpty(GitHubAuthToken))
			throw new Exception("You must set a github.com auth token in auth.targets (see auth.targets.example)");

		var tag = "v" + version;
		Git.Tag(tag);
		Git.Push("origin", tag);

		var filesInZip = new List<string> { "Build.csx", "README.md" };

		filesInZip.AddRange(Files.GetFilesWithPattern("doc", "*.*", true));
		filesInZip.AddRange(Files.GetFilesWithPattern("examples", "*.*", true));
		filesInZip.AddRange(Files.GetFilesWithPattern("modules", "*.*", true));
		filesInZip.AddRange(Files.GetFilesWithPattern("tools", "*.*", true));
		
		var zipFile = softwareName + "_" + tag + ".zip";
		Zip.ZipFilesInArchive(zipFile, filesInZip.ToArray());
		
		var github = new GitHubWrapper.GitHubWrapper();
		github.Release(GitHubLogin + "/" + softwareName, GitHubAuthToken, tag, new UploadFile[]{ new UploadFile{ Path = zipFile, ContentType = "application/zip" } });
	}

	void Clean(bool keepResultFiles)
	{
		Console.WriteLine("Cleaning files & Folders...");
		Files.DeleteFilesWithPattern(".", "*.zip");
	}
}
