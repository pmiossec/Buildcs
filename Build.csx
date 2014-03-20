//Usage:
//scriptcs.exe build.csx --
//scriptcs.exe build.csx -- /t:Build /o:other /a:arguments

public class Build
{
	public static bool DebugEnabled = false;
	public static string Now { get { return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); } }
	private static string logFile = "build_log_" + Now + ".txt";
	public static string LogFile { get { return logFile; } }
	public static bool LogEnabled = true;

	private static bool _areArgumentsInitialized = false;
	//Used to initialize the command line arguments
	public static void SetScriptArguments(IReadOnlyList<string> scriptArguments)
	{
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
	public static string Target
	{
		get
		{
			if(!_areArgumentsInitialized)
				throw new Exception("Please Add the line 'Build.SetScriptParameters(Env.ScriptArgs);' before calling 'RunTarget()' method!");

			return _target;
		}
	}

	static List<string> _arguments;
	public static List<string> Arguments { get { return _arguments; } }

	//Get command line argument
	public static string GetArguments(string prefix, string defaultValue)
	{
		var argument = Arguments.FirstOrDefault(a =>!a.StartsWith(prefix));
		if(argument == null)
			return defaultValue;
		return argument.Substring(prefix.Length);
	}

	public void RunTarget()
	{
		var targets = this.GetType().GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
			.Where(m => m.GetCustomAttributes(typeof(TargetAttribute), false).Length > 0);

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
			DisplayAndLog("Target '" + Target + "' not found!");
		else
		{
			try
			{
				DisplayAndLog("Running target:" + method.Name);
				method.Invoke(this, null);
			}
			catch(Exception ex)
			{
				DisplayAndLog("Build failed with error:" + ex.InnerException.Message);
				System.Environment.Exit(1);
			}
		}
	}

	//Run a command line task!
	public static bool RunTask(string command, string arguments = null, bool continueOnError = false)
	{
		DisplayAndLog("Running command:" + command + " " + arguments);
		var process = new System.Diagnostics.Process();
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.Start();
		Log(process.StandardOutput.ReadToEnd());
		process.WaitForExit();

		if(!continueOnError && process.ExitCode != 0)
			throw new Exception("Process exit with error! Please consult log file...");

		DisplayAndLog("Process run successfully!");
		return process.ExitCode == 0;
	}

	//Display a string in the console and the log file
	public static void DisplayAndLog(string log)
	{
		System.Console.WriteLine(log);
		Log(Now + ":" + log+"\n");
	}

	//Write a string in the log
	public static void Log(string log)
	{
		if(LogEnabled)
			File.AppendAllText(logFile, log);
	}

	//Get the value of an environnement variable
	public static string GetEnvironnementVariable(string variable)
	{
		var envVar = Environment.GetEnvironmentVariable(variable);
		Debug(string.Format("[ENV_VAR]{0}={1}", variable, envVar));
		return envVar;
	}

	public static void Debug(string log)
	{
		if(DebugEnabled)
			Log("[DEBUG]" + log);
	}
}

public class TargetAttribute : Attribute
{
}