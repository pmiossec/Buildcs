# Write your first build script

Here is the minimum viable build script `MyBuild.csx` (Note that C# comments are not allowed before the last line beginning by a '#' and are here for comprehension):

		//Include the main file of Buildcs
		#load Build.csx
		
		//Bootstrap: Run the selected targets (or the only existing one)
		//Must be called with the type of your build class and 'Env.ScriptArgs' as parameters
		BuildHelper.RunTarget(typeof(MyBuild), Env.ScriptArgs);

		//Create a class where you define your build configuration
		//This class MUST inherite the BuildHelper class and have a no parameter constructor!
		public class MyBuild : BuildHelper
		{
			//Define at least one method with the attribute [Target]
			//which define methods that your could run
			[Target]
			void HelloWorld()
			{
				//Define the action that must be done in this target
				Console.WriteLine("Hello, world!");
				
				//Launch an external command
				RunTask("echo", "Hello, too!");
				
				//... but you could also call other methods
				//and organize your tasks as you wish!
			}
		}

You could call your script using the command (if you have only one [Target] or want to run the first one of the file):

		scriptcs MyBuild.csx

or specifying the target (when you have more than one [Target]):

		scriptcs MyBuild.csx -- /t:HelloWorld
		
Note:

* Scriptcs use the separator `--` to specify the arguments that are passed to the script (the ones after). The one given before are used by Scriptcs.
* Buildcs use the option '/t:' to define the [Target] used
		
# Built-in features

You could get an up to date help on the API by launching the target "Help":

		scriptcs MyBuild.csx -- /t:Help

* Get one argument of the build script

		var configuration = GetArguments("/config:", "Release");

The first parameter is the prefix `/prefix:` to find the argument, the second is the default value if no parameter is found.

Note: the only prefix used is `/t:` to specify the target. The other arguments prefix are up to you ;) 


* Launch a process

		var success = RunTask("commande.exe", "arguments", false);

Note: The 3rd parameter tell if the build should continue if an error occurs
(the command return an exit code different of 0)

* Write in the Log file

		//Display a string in the console and the log file
		DisplayAndLog("Display in Console AND Logs");

		//Write a string in the log
		Log("Display in Logs only")

* read environnement variable

		var path = GetEnvironnementVariable("PATH");

* Debug your build script

You could enable debug logs using the code :

	BuildHelper.DebugEnabled = true;

and using the method in your script:

	Debug("Oh, a debug message");

* Disable log file

		BuildHelper.LogDisabled = true;


* You could use [external modules](Modules.md) (like `File.csx`) by using `#load File.csx`
* Use of your class constructor to run some initialization or pre-cleaning.
* Use of your class finalizer to run some post-cleaning (even if your build fail!).
* All that is permitted by [Scriptcs](http://scriptcs.net/). You will find good documentation there!

# Advice

* Do not hesitate to create one method for each task to do and call theses methods in your different targets!
* See [SampleBuild.csx](../examples/SampleBuild.csx) for an example...


# Some help on Scriptcs

* Load another script using: `#load myModule.csx`
* Include an assembly using: `#r MyCompany.MyProduct.Data.dll`
* Loading of a script must be done before including an assembly
* Comments '//' are not allowed before or in the lines beginning with '#', so, never comment one of these lines or put them last.
* Be really careful with parenthesis '(', ')' and curly brackets '{', '}'
because Scriptcs return no errors during compilation and do nothing if one is missing :(
* Scriptcs take ~10 seconds to compile the script before running it, so wait ;) 

# Modules

You can extand the feature of Buildcs with external modules

## List of the existing modules

* [Files.csx](../modules/Files.csx): Manage files and directories
* [MsBuild.csx](../modules/MsBuild.csx): Run MsBuild
* [MsTest.csx](../modules/MsTest.csx): Run MsTest (Microsoft unit test framework)
* [VsTest.csx](../modules/VsTest.csx): Run VsTest (New runtime for Microsoft unit test framework)
* [OpenCover.csx](../modules/OpenCover.csx): Run [Opencover](https://github.com/OpenCover/opencover)
* [Git.csx](../modules/Git.csx): Manage a Git repository
* [Zip.csx](../modules/Zip.csx): Zip files in an archive

## Write a module

To write your own module (that you can contribute by a pull request), you just have to write a class with `static` methods.

		using System.IO.Compression;

		public class MyModule
		{
			public static void MyMethod(string parameter)
			{
				//What I want to do
				
				//If you want to Launch a process
				BuildHelper.RunTask("myprocess.exe", "arguments");
				
				//or call a methods of Buildcs
				BuildHelper.DisplayAndLog("This is a message!!!);
			}
		}
