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
	[Display(Description = "Enable/Disable debug logging.")]
	public static bool DebugEnabled { get; set; }

	[Display(Description = "String representative of the date 'Now'.")]
	public static string Now { get { return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); } }

	[Display(Description = "Name of the log file.")]
	public static string LogFileName = "build_log_" + Now + ".txt";

	[Display(Description = "Path of the build log file.")]
	public static string LogFile { get { return Path.Combine(LogPath?? string.Empty, LogFileName); } }

	[Display(Description = "Enable/Disable logging.")]
	public static bool LogDisabled { get; set; }

	[Display(Description = "Path of the log file. Set MUST be done before RunTarget() call!")]
	public static string LogPath { get; set; }

	[Display(Description = "Get the console output of the last task command run")]
	public static string LastTaskOutput { get; private set; }

	[Display(Description = "Set if the script should continue when an error is encountered or a command exit with an error code.")]
	public static bool ContinueOnError { get; set; }

	[Display(Description = "Display all the log of the run commands in the console output.")]
	public static bool LogCommandsOnConsole { get; set; }

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

		_areArgumentsInitialized = true;
	}

	private static string _target = null;
	[Display(Description = "Names of the called target.")]
	public static string Target { get { return _target; } }
	private List<MethodInfo> Targets
	{
		get
		{
			return this.GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(TargetAttribute), false).Length > 0).ToList();
		}
	}

	[Display(Description = "Names of the available targets.")]
	public List<string> TargetNames { get { return Targets.Select(m =>m.Name).ToList(); } }

	[Display(Description = "Main Method that MUST be called to Launch your [Target]. ex: BuildHelper.RunTarget(typeof(MyBuild),Env.ScriptArgs)")]
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
			DisplayAndLog("Target '" + Target + "' not found!", DisplayLevel.Error);
			DisplayAndLog("   => Verify the name of the target or add the attribute [Target] to the method...", DisplayLevel.Error);
			DisplayLine("      Available targets: " + string.Join(", " , customBuild.TargetNames), DisplayLevel.Debug);
		}
		else
		{
			try
			{
				DisplayAndLog("Running target:" + method.Name, DisplayLevel.Success);
				DisplayAndLog("Arguments:" + (Arguments.Any() ? string.Join("," , Arguments) : "(none)"), DisplayLevel.Debug);
				DisplayLine("   => Build log file: " + Path.GetFullPath(LogFile), DisplayLevel.Warning);
				
				LogCommandsOnConsole = bool.Parse(BuildHelper.GetArguments("/verbose:", "False"));
				
				Time(() => {
					method.Invoke(customBuild, null);
					Console.WriteLine();
				}, "Total ");
				DisplayAndLog("Build status: OK", DisplayLevel.Success);
				DisplayLine("   => Build log file: " + Path.GetFullPath(LogFile), DisplayLevel.Warning);
			}
			catch(Exception ex)
			{
				DisplayAndLog("error: " + ex.InnerException.Message, DisplayLevel.Error);
				DisplayAndLog("Build status: KO", DisplayLevel.Error);
				DisplayLine("   => Build log file: " + Path.GetFullPath(LogFile), DisplayLevel.Warning);
				System.Environment.Exit(1);
			}
		}
	}

	[Target]
	[Display(Description = "Default [Target] to display some help.")]
	public void Help()
	{
		DisplayAndLog("Available targets: " + string.Join(", " , TargetNames), DisplayLevel.Debug, false);
		DisplayAndLog();
		DisplayAndLog("Properties:", DisplayLevel.Warning, false);
		DisplayAndLog();
		foreach(var property in typeof(BuildHelper).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))
		{
			if(!DisplayComment(property))
				continue;
			DisplayAndLog("  " + property.PropertyType + " " + property.Name + " {" + (property.CanRead ? " get;" : string.Empty) + (property.CanWrite ? " set;" : string.Empty) + " }", DisplayLevel.Info , false);
			DisplayAndLog();
		}

		Console.WriteLine();
		DisplayAndLog("Methods:", DisplayLevel.Warning, false);
		DisplayAndLog();
		foreach(var method in typeof(BuildHelper).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)
			.Where(x => !x.IsSpecialName))
		DisplayMethod(method);
	}

	private bool DisplayComment(MemberInfo method)
	{
		var comment = method.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault();
		if(comment != null)
		{
			DisplayAndLog("* " + ((DisplayAttribute)comment).Description, DisplayLevel.Success, false);
			return true;
		}

		return false;
	}

	private void DisplayMethod(MethodInfo method)
	{
		if(!DisplayComment(method))
			return;
		var parameters = string.Join(", ", method.GetParameters().Select(p => {
			string defaultParam = string.Empty;
			if(p.IsOptional)
			{
				string defaultValue = string.Empty + p.DefaultValue;
				defaultParam = " = " + (string.IsNullOrEmpty(defaultValue) ? "null" : defaultValue);
			}

			return p.ParameterType + " " + p.Name + defaultParam;
		}

			));
		DisplayAndLog("  " + method.Name+"(" + parameters + ")", DisplayLevel.Info, false);
		DisplayAndLog();
	}

	static List<string> _arguments;
	[Display(Description = "Arguments passed to the build script (except the target).")]
	public static List<string> Arguments { get { return _arguments; } }

	[Display(Description = "Method to call to get the value of a script argument. If no argument found, the default value is returned.")]
	public static string GetArguments(string prefix, string defaultValue)
	{
		var argument = Arguments.FirstOrDefault(a =>a.StartsWith(prefix));
		if(argument == null)
			return defaultValue;
		return argument.Substring(prefix.Length);
	}

	[Display(Description = "Method to call to Launch a process.")]
	public static bool RunTask(string command, string arguments = null, bool displayInLog = true)
	{
		StringBuilder outputBuilder = new StringBuilder();
		DisplayAndLog(string.Empty);
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
			if(LogCommandsOnConsole)
				Console.WriteLine(e.Data);
		});

		Time(() => {
			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();
			process.CancelOutputRead();
		}, "  =>");

		LastTaskOutput = outputBuilder.ToString().TrimEnd('\n', '\r');

		if(!ContinueOnError && process.ExitCode != 0)
		{
			Console.WriteLine(LastTaskOutput);
			throw new Exception("  =>Process exited with an error!");
		}

		if(process.ExitCode == 0)
		{
			if(displayInLog)
				DisplayAndLog("  =>Process run successfully!", DisplayLevel.Success);
			return true;
		}

		DisplayAndLog("  =>Process exited with an error :(", DisplayLevel.Error);
		return false;
	}

	[Display(Description = "Method to call to get the value of an environnement variable.")]
	public static string GetEnvironnementVariable(string variable)
	{
		var envVar = Environment.GetEnvironmentVariable(variable);
		Debug(string.Format("[ENV_VAR]{0}={1}", variable, envVar));
		return envVar;
	}

	[Display(Description = "Method to call to display a string in the console and the log file.")]
	public static void DisplayAndLog(string log = null, DisplayLevel displayLevel = DisplayLevel.Info, bool timeStamp = true)
	{
		DisplayLine(log, displayLevel);
		Log(((log == null) ? string.Empty : (timeStamp ? (Now + ":") : string.Empty) + log) + "\n");
	}

	[Display(Description = "Method to call to display a string in the console with color depending enum 'DisplayLevel'.")]
	public static void DisplayLine(string text, DisplayLevel displayLevel = DisplayLevel.Info)
	{
		switch(displayLevel)
		{
			case DisplayLevel.Info :
				Console.ResetColor();
			break;
			case DisplayLevel.Debug :
				Console.ForegroundColor = ConsoleColor.Blue;
			break;
			case DisplayLevel.Warning :
				Console.ForegroundColor = ConsoleColor.Yellow;
			break;
			case DisplayLevel.Error :
				Console.ForegroundColor = ConsoleColor.Red;
			break;
			case DisplayLevel.Success :
				Console.ForegroundColor = ConsoleColor.Green;
			break;
		}

		Console.WriteLine(text);
		Console.ResetColor();
	}

	[Display(Description = "Method to call to write a string in the log.")]
	public static void Log(string log)
	{
		if(!LogDisabled)
			File.AppendAllText(LogFile, log);
	}

	[Display(Description = "Write a Debug log.")]
	public static void Debug(string log)
	{
		if(DebugEnabled)
			Log("[DEBUG]" + log);
	}

	[Display(Description = "Method to call to easily build command parameters.")]
	public static string BuildCommand(params string[] parameters)
	{
		return string.Join(" ", parameters);
	}

	[Display(Description = "Method to call to time the duration of a method.")]
	public static void Time(Action action, string prefix = null)
	{
		Stopwatch st = new Stopwatch();
		st.Start();
		action();
		st.Stop();
		DisplayAndLog(string.Format("{0}Duration:{1}", prefix , st.Elapsed.ToString("mm\\:ss\\.ff")));
	}

	[Display(Description = "Method to call to pause the process when debugging and waiting for user action to restart.")]
	public static void PauseAndWaitForUser()
	{
		Console.WriteLine("Build process paused... (Press 'enter' to continue)");
		Console.ReadLine();
	}

	public static T ContinueOrFail<T>(Func<T> action)
	{
		try
		{
			return action();
		}
		catch(Exception ex)
		{
			if(ContinueOnError)
			{
				BuildHelper.DisplayAndLog("error: " + ex.Message, DisplayLevel.Error);
				BuildHelper.DisplayAndLog("Continue anyway...", DisplayLevel.Warning);
				return default(T);
			}
			throw;
		}
	}

	public static void ContinueOrFail(Action action)
	{
		ContinueOrFail(()=>{ action(); return true;});
	}
}

public enum DisplayLevel
{
	Info,
	Debug,
	Warning,
	Error,
	Success
}

public class TargetAttribute : Attribute
{
}