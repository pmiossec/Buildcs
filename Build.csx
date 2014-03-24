#r System.ComponentModel.DataAnnotations.dll
//Usage:
//scriptcs.exe build.csx --
//scriptcs.exe build.csx -- /t:Build /o:other /a:arguments
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;

public class BuildHelper
{
	public static bool DebugEnabled = false;
	public static string Now { get { return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); } }
	private static string logFile = "build_log_" + Now + ".txt";
	public static string LogFile { get { return logFile; } }
	public static bool LogEnabled = true;

	private static bool _areArgumentsInitialized = false;
	[Display(Description = "Method used to set script arguments.")]
	private static void SetScriptArguments(IReadOnlyList<string> scriptArguments)
	{
		if(scriptArguments == null)
		{
			DisplayAndLog("The 'RunTarget()' method must be called with 'Env.ScriptArgs' as parameter! Please add the parameter.");
			System.Environment.Exit(1);		}

		var prefix = "/t:";
		var targets = scriptArguments.Where(a =>a.StartsWith(prefix));
		if(targets.Count() == 1)
			_target = targets.First().Substring(prefix.Length);;
		if(targets.Count() > 1)
			throw new Exception(string.Format("You should specify only 1 target with option '{0}'!", prefix));

		_arguments = scriptArguments.Where(a =>!a.StartsWith(prefix)).ToList();
		DisplayAndLog("Arguments:" + (Arguments.Any() ? string.Join("," , Arguments) : "(none)"));

		_areArgumentsInitialized = true;
	}

	private static string _target = null;
	public static string Target { get { return _target; } }
	private List<MethodInfo> Targets
	{
		get
		{
			return this.GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(TargetAttribute), false).Length > 0).ToList();
		}
	}

	public List<string> TargetNames { get { return Targets.Select(m =>m.Name).ToList(); } }

	[Target]
	[Display(Description = "Default [Target] to display some help.")]
	public void Help()
	{
		Console.WriteLine("Available targets: " + string.Join(", " , TargetNames));
		Console.WriteLine();
		Console.WriteLine("Properties:");
		Console.WriteLine("-----------");
		Console.WriteLine();
		foreach(var property in typeof(BuildHelper).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))
			Console.WriteLine(property.PropertyType + " " + property.Name + "{" + (property.CanRead ? " get;" : string.Empty) + (property.CanWrite ? " set;" : string.Empty) + "}");
		Console.WriteLine();
		Console.WriteLine("Methods:");
		Console.WriteLine("--------");
		Console.WriteLine();
		foreach(var method in typeof(BuildHelper).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
			.Where(x => !x.IsSpecialName))
		DisplayMethod(method);
	}

	private void DisplayMethod(MethodInfo method)
	{
		var comment = method.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault();
		if(comment != null)
			Console.WriteLine("* " + ((DisplayAttribute)comment).Description);
		Console.WriteLine("  " + method.Name+"(" + string.Join(", ", method.GetParameters().Select(p =>p.ParameterType + " " + p.Name)) + ")");
		Console.WriteLine();
	}

	static List<string> _arguments;
	public static List<string> Arguments { get { return _arguments; } }

	[Display(Description = "Method to call to get the value of a script argument. If no argument found, the default value is returned.")]
	public static string GetArguments(string prefix, string defaultValue)
	{
		var argument = Arguments.FirstOrDefault(a =>a.StartsWith(prefix));
		if(argument == null)
			return defaultValue;
		return argument.Substring(prefix.Length);
	}

	[Display(Description = "Main Method to call to Launch your [Target]. Call it with 'Env.ScriptArgs' as parameter ex: new MyBuild().RunTarget(Env.ScriptArgs)")]
	public static void RunTarget(Type type, IReadOnlyList<string> scriptArguments = null)
	{
		SetScriptArguments(scriptArguments);

		var customBuild = (BuildHelper)Activator.CreateInstance(type);
		var targets = customBuild.Targets;

		System.Reflection.MethodInfo method;
		if(string.IsNullOrEmpty(Target))
		{
			method = targets.FirstOrDefault();
			if(method == null)
			{
				DisplayAndLog("No [Target] defined!");
				return;
			}
		}
		else
		{
			method = targets.Where(m => m.Name == Target).FirstOrDefault();
		}

		if (method == null)
		{
			DisplayAndLog("Target '" + Target + "' not found!");
			DisplayAndLog("   => Verify the name of the target or add the attribute [Target] to the method...");
		}
		else
		{
			try
			{
				DisplayAndLog("Running target:" + method.Name);
				Console.WriteLine("   => Build log file: " + logFile);
				method.Invoke(customBuild, null);
			}
			catch(Exception ex)
			{
				DisplayAndLog("Build failed with error:" + ex.InnerException.Message);
				System.Environment.Exit(1);
			}

			finally
			{
				Console.WriteLine("   => Build log file: " + logFile);
			}
		}
	}

	[Display(Description = "Method to call to Launch a process.")]
	public static bool RunTask(string command, string arguments = null, bool continueOnError = false)
	{
		string output;
		return RunTask(out output, command, arguments, continueOnError, true);
	}

	[Display(Description = "Method to call to Launch a process with the console ouput.")]
	public static bool RunTask(out string output, string command, string arguments = null, bool continueOnError = false, bool displayInLog = false)
	{
		StringBuilder outputBuilder = new StringBuilder();
		DisplayAndLog("Running command:" + command + " " + arguments);
		var process = new System.Diagnostics.Process();
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.WorkingDirectory = ".";
		process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
			outputBuilder.AppendLine(e.Data);
			if(displayInLog)
				Log(e.Data + "\n");
		});
		
		Time(() => {
			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();
			process.CancelOutputRead();
		});

		if(!continueOnError && process.ExitCode != 0)
		{
			Console.WriteLine(outputBuilder.ToString().TrimEnd('\n', '\r'));
			throw new Exception("Process exited with an error! Please consult log file ( " + logFile + ")...");
		}

		output = outputBuilder.ToString().TrimEnd('\n', '\r');
		if(process.ExitCode == 0)
		{
			if(displayInLog)
				DisplayAndLog("Process run successfully!");
			return true;
		}
		DisplayAndLog("Process exited with an error :(");
		return false;
	}

	[Display(Description="Method to call to pause the process when debugging and waiting for user action to restart.")]
	public static void PauseAndWaitForUser()
	{
		Console.WriteLine("Build process paused... (Press 'enter' to continue)");
		Console.ReadLine();
	}

	[Display(Description="Method to call to display a string in the console and the log file.")]
	public static void DisplayAndLog(string log)
	{
		System.Console.WriteLine(log);
		Log(Now + ":" + log+"\n");
	}

	[Display(Description="Method to call to write a string in the log.")]
	public static void Log(string log)
	{
		if(LogEnabled)
			File.AppendAllText(logFile, log);
	}

	[Display(Description="Method to call to get the value of an environnement variable.")]
	public static string GetEnvironnementVariable(string variable)
	{
		var envVar = Environment.GetEnvironmentVariable(variable);
		Debug(string.Format("[ENV_VAR]{0}={1}", variable, envVar));
		return envVar;
	}

	[Display(Description="Write a Debug log.")]
	public static void Debug(string log)
	{
		if(DebugEnabled)
			Log("[DEBUG]" + log);
	}

	[Display(Description="Method to call to easily build command parameters.")]
	public static string BuildCommand(params string[] parameters)
	{
		return string.Join(" ", parameters);
	}

	[Display(Description="Method to call to time the duration of a method.")]
	public static void Time(Action action)
	{
		Stopwatch st = new Stopwatch();
		st.Start();
		action();
		st.Stop();
		DisplayAndLog("Duration:" + st.Elapsed.ToString("mm\\:ss\\.ff"));
	}
}

public class TargetAttribute : Attribute
{
}