#load Build.csx
#load Files.csx
#r "Microsoft.Build.dll"
//Usage:
//scriptcs.exe sample_build.csx --
//scriptcs.exe sample_build.csx -- /t:Build /c:Debug /b:MyBranch
using System.Xml;

//Enable/Disable log (default, true)
Build.LogEnabled = true;
//Set script arguments (Never remove!)
Build.SetScriptArguments(Env.ScriptArgs);
//Run selected target of your custom build definition
new SampleBuild().RunTarget();

//This class define your custom build (and MUST inherit from 'Build' class)
public class SampleBuild : Build
{
	//Define here, variables common to different targets
	static string ProjectDir = @".\";
	static string ProjectTestDir = @".\";
	static string ProjectFile = ProjectDir + @"MySolution.sln";
	static string OutputDir = @"bin\" + Configuration;

	//Get some values from script arguments ( prefix of option is up to you --except '/t:' used for determine targets)
	static string Configuration = GetArguments("/c:", "Release");

	//Create methods and Add '[Target]' attribute to the one you want to be able to launch using '/t:' argument.
	//ex: scriptcs.exe sample_build.csx -- /t:Help
	//If you don't specify a target in the command line, the first one will be chosen!
	[Target]
	void Help()
	{
		System.Console.WriteLine(Configuration);
	}

	[Target]
	void RunAll()
	{
		//Organize what a target do and which actions are done calling other methods
		Build();
		CoverageAndTests();
		Publish();
		Clean();
	}

	[Target]
	void Build()
	{
		//Use RunTask() helper to launch a process
		RunTask("msbuild.exe", ProjectFile + " /m");
	}

	void CoverageAndTests()
	{
		var CoverageReportFolder = "coverage_report";
		var CoverageReportFile = CoverageReportFolder + ".xml";

		try
		{
			Files.DeleteFile(CoverageReportFile);
			var success = RunTask("OpenCover.Console.exe", "-register:user -filter:+[MyProject.Assemblies.*]* -output:" + CoverageReportFile +" -target:" + MsTestPath + " -targetargs:\"" + MsTestParameters + " " + MsTestIntegrationAssemblies +"\"", true);
			if(success) RunTask("reportgenerator.exe", CoverageReportFile + " " + CoverageReportFolder);
			Files.DeleteFile(CoverageReportFile);
		}
		finally
		{
			CleanTestFolders();
		}
	}

	
	void Publish()
	{
		Files.CopyFolder(LocalPublishFolder, PublishFolder);
		Files.DeleteDirectory(LocalPublishFolder);
	}

	void Clean()
	{
		Files.DeleteFile(TestResultFile);
		Files.DeleteFilesWithPattern("TestResults", "*.trx");
		Files.DeleteDirectoriesWithPattern(".", string.Format("{0}_{1}*", GetEnvironnementVariable("USERNAME"), GetEnvironnementVariable("COMPUTERNAME")));
	}
	
	///----------Builtin Methods:
	//Get command line argument
	string GetArguments(string prefix, string defaultValue)
	//Run a command line task!
	bool RunTask(string command, string arguments, bool continueOnError = false)
	//Display a string in the console and the log file
	void DisplayAndLog(string log)
	//Write a string in the log
	void Log(string log)
	//Get the value of an environnement variable
	string GetEnvironnementVariable(string variable)
}

