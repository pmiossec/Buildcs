#r "Microsoft.Build.dll"
#load ..\Build.csx
#load ..\modules\Files.csx
#load ..\modules\MsBuild.csx
#load ..\modules\Git.csx
#load ..\modules\MsTest.csx
#load ..\modules\VsTest.csx
#load ..\modules\OpenCover.csx
//Usage:
//scriptcs.exe .\Build\SampleBuild.csx
//scriptcs.exe .\Build\SampleBuild.csx -- /t:Build /c:Release /tests:Unit /f:VsTest
using System.Xml;

BuildHelper.LogPath = @".\Build";
BuildHelper.RunTarget(typeof(MyBuild), Env.ScriptArgs);

public class MyBuild : BuildHelper
{
	string _configuration;
	//Get some values from script arguments ( prefix of option is up to you --except '/t:' used for determine targets)
	string Configuration { get { return _configuration ?? (_configuration = BuildHelper.GetArguments("/c:", "Release")); } }
	string ScopeTests { get { return BuildHelper.GetArguments("/tests:", "Unit"); } }
	string FrameworkTests { get { return BuildHelper.GetArguments("/f:", "VsTest"); } }

	string OutputDir { get { return @"bin\" + Configuration; } }
	string ProjectDir = @".\";
	string ProjectTestDir = @".\";
	string ProjectFile { get { return ProjectDir + @"Project.sln"; } }
	string LocalPublishFolder = "ClickOnce";

	string VsToolsPath = GetEnvironnementVariable("VS110COMNTOOLS") + @"..\IDE\";
	string TestResultFile = "TestResults.trx";
	List<string> TestAssemblies = new List<string>
	{
		@".\Test.Unit\{0}\Test.Unit.dll",
		@".\Test.Common\{0}\Test.Common.dll",
		@".\Test.Integration\{0}\Test.Integration.dll",
	};

	List<string> AssembliesOnTest
	{
		get
		{
			return ((ScopeTests == "Unit") ? TestAssemblies.Take(1) : TestAssemblies)
					.Select(a => string.Format(a, OutputDir)).ToList();
		}
	}

	static string CoverageReportFolder = "coverage_report";
	static string CoverageReportFile = CoverageReportFolder + ".xml";

	public MyBuild()
	{
		MsTest.PathToExe = VsToolsPath;
		VsTest.PathToExe = VsToolsPath + @"CommonExtensions\Microsoft\TestWindow\";
		Clean(keepResultFiles:false);
	}

	~MyBuild() { Clean(keepResultFiles:true); }

	//Create methods and Add '[Target]' attribute to the methods you want to be able to launch using '/t:' argument.
	//ex: scriptcs.exe SampleBuild.csx -- /t:CleanBuild
	//If you don't specify a target in the command line, the first one will be chosen!
	[Target]
	void CleanBuild()
	{
		MsBuild.Clean(ProjectFile, Configuration);
	}

	[Target]
	void FindTestAssemblies()
	{
		var testAssemblies = Files.GetFilesWithPattern(".", @"Test.*.dll", true);
		foreach(var ass in testAssemblies)
			Console.WriteLine(ass);
	}

	[Target]
	void Build()
	{
		MsBuild.Run(ProjectFile, Configuration);
	}

	[Target]
	void UnitTests()
	{
		Build(); _RunUnitTests();
	}

	[Target]
	void IntegrationTests()
	{
		Build(); _RunIntegrationTests();
	}

	[Target]
	void Coverage()
	{
		Build(); _Coverage();
	}

	[Target]
	void Measure()
	{
		Build(); _Coverage(); Measure();
	}

	[Target]
	void Publish()
	{
		_configuration = "Release"; GitStamp(); _Publish();
	}

	[Target]
	void Package()
	{
		_configuration = "Release"; CleanBuild(); GitStamp(); Build(); _Package();
	}

	void GitStamp()
	{
		Files.ReplaceText(@".\Solution Items\VersionInfo.cs", @"(assembly: AssemblyFileVersion[^+\n]*)( \+[^\n]*)?\)\]", "$1 + \"_" + Git.CurrentSha() + "\")]");
	}

	void _RunUnitTests()
	{
		MsTest.Run(TestAssemblies.Take(1).Select(a => string.Format(a, OutputDir)), TestResultFile, "local.testsettings");
	}

	void _RunIntegrationTests()
	{
		MsTest.Run(TestAssemblies.Select(a => string.Format(a, OutputDir)), TestResultFile, "local.testsettings");
	}

	void _Coverage()
	{
		bool success;
		if(FrameworkTests == "VsTest")
		{
			var vsTestParameters = VsTest.GetParameters(AssembliesOnTest, TestResultFile, "local.testsettings");
			success = OpenCover.Run("+[Company.Project.*]*", VsTest.FullPathExe, vsTestParameters, CoverageReportFile);
		}
		else
		{
			var msTestParameters = MsTest.GetParameters(AssembliesOnTest, TestResultFile, "local.testsettings");
			success = OpenCover.Run("+[Company.Project.*]*", MsTest.FullPathExe, msTestParameters, CoverageReportFile);
		}

		if(success) RunTask("reportgenerator.exe", CoverageReportFile + " " + CoverageReportFolder);
	}

	void _Measure()
	{
		RunTask("gendarme.exe", @" --xml gendarme_report.xml .\Chemin\Vers\Mon\Assembly\Mon.Assembly2.dll");
		RunTask("simian.exe", @" **/*.cs -formatter=xml:simian-output.xml -excludes=**/*.xaml.cs");
		RunTask("fxcopcmd.exe", @"/out:fxcop-result.xml /file:"".\Company.Project\" + OutputDir + @"\Company.*.*"" /directory:"".\Company.Project\" + OutputDir + @""" /directory:""C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"" /rule:UsageRules.dll /rule:DesignRules.dll /rule:GlobalizationRules.dll /rule:InteroperabilityRules.dll /rule:MobilityRules.dll /rule:NamingRules.dll /rule:PerformanceRules.dll /rule:PortabilityRules.dll /rule:SecurityRules.dll", true);
		RunTask("StyleCopCLI.exe", @" -sln Project.sln -out StyleCopResults.xml -set Settings.StyleCop");
	}
	
	void _Publish()
	{
		var Project2Publish = @".\Company.Project\Company.Project.csproj";
		var PublishFolder = @"\\Server\Project-Deploy\";

		MsBuild.Run(ProjectFile, "Release", true, "Build;Publish", @";PublishDir=..\" + LocalPublishFolder +@"\;PublishUrl="+ PublishFolder + ";InstallUrl=" + PublishFolder + ";UpdateUrl="+ PublishFolder);

		//Update project file!
		var project = new Microsoft.Build.Evaluation.Project(Project2Publish);
		var property = project.GetProperty("ApplicationRevision");
		property.UnevaluatedValue = "" + (System.Int32.Parse(property.EvaluatedValue) + 1);
		project.Save();

		Files.CopyFolder(LocalPublishFolder, PublishFolder, true, true);
	}

	void _Package()
	{
		MsBuild.Run(@".\Company.Project.Setup\Company.Project.Setup.wixproj", "Release", true);
	}

	void Archive()
	{
		//ZipFilesInArchive("build_" + Now + ".zip", LogFile, CoverageReportFile, "gendarme_report.xml", "simian-output.xml", "fxcop-result.xml", "StyleCopResults.xml");
	}

	void Clean(bool keepResultFiles)
	{
		if(keepResultFiles)
		{
			Files.DeleteFile(TestResultFile);
			Files.DeleteDirectory(CoverageReportFolder);
		}
		Files.DeleteFile(CoverageReportFile);
		Files.DeleteFilesWithPattern("TestResults", "*.trx");
		Files.DeleteDirectoriesWithPattern(".", string.Format("{0}_{1}*", GetEnvironnementVariable("USERNAME"), GetEnvironnementVariable("COMPUTERNAME")));
		Files.DeleteFile(ProjectTestDir + TestResultFile);
		Files.DeleteDirectory(LocalPublishFolder);
	}
}

