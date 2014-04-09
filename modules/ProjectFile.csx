#r "Microsoft.Build.dll"
using System.Xml;

public static partial class CsProjFile
{
	public static void UpdateApplicationRevision(string projectFilePath, int? newValue = null)
	{
		BuildHelper.ContinueOrFail(() => {
			var project = new Microsoft.Build.Evaluation.Project(projectFilePath);

			var property = project.GetProperty("ApplicationRevision");
			if(newValue.HasValue)
				property.UnevaluatedValue = newValue.Value.ToString();
			else
				property.UnevaluatedValue = (System.Int32.Parse(property.EvaluatedValue) + 1).ToString();
			project.Save();
		});
	}
}