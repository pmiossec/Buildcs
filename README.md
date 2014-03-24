Buildcs
=======

Build Automation Tool for C# developpers (using Scriptcs, scripts written in C#)

# Introduction

There is plenty of tools to automate builds in every language. In .Net world, we have the (bad) old msbuild (horrible xml), psake (powershell) or FAKE (F#).

But here is the Build Automation Tool written in C# (with all the power of the .Net framework). Because, as a .Net developper, that's the language you master the best ;)

# Features

* Define easily the tasks run in C#
* Define multiple targets in the same file
* Extend very easily with your own modules

# Getting started

* Install [Scriptcs](http://scriptcs.net/).

* Download the content of this repository (or clone the repository).

* Create a file called `MyBuild.csx` with the content:


		#load Build.csx
		new MyBuild().RunTarget(Env.ScriptArgs);
	
		public class MyBuild : Build
		{
			[Target]
			void HelloWorld()
			{
				Console.WriteLine("Hello, world!");
				
				RunTask("echo", "Hello, too!");
			}
		}


* Run it with command: `scriptcs MyBuild.csx`

# Documentation

Read at least this [short documentation](doc/Buildcs.md) if you plan to use it.



