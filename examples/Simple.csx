#load Build.csx
#load Files.csx
#load Git.csx

BuildHelper.RunTarget(typeof(MyBuild), Env.ScriptArgs);

public class MyBuild : BuildHelper
{
	public MyBuild()
	{
		Console.WriteLine("Constructor!");
	}

	[Target]
	void HelloWorld()
	{
		//var path = GetEnvironnementVariable("PATH");
		//Console.WriteLine(GetEnvironnementVariable("PATH"));
		//throw new Exception("test exception");
		RunTask("echo", "Hello, too!");
	}

	
	~MyBuild()
	{
		Console.WriteLine("Destructor!");
	}
	
}

