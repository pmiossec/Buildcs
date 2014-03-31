using System;
using Microsoft.VisualStudio.Coverage.Analysis;

namespace VsTestCoverageConverter
{
	//https://reportgenerator.codeplex.com/wikipage?title=Visual%20Studio%20Coverage%20Tools
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args.Length > 1)
					VsTestCoverageConverter.ConvertCoverageFile(args[0], args[1]);
				else
					VsTestCoverageConverter.ConvertCoverageFile(args[0]);
			}
			else
			{
				Console.WriteLine("Usage:\n    VsTestCoverageConverter.exe vstest_coverage_file.coverage [coverage_report.xml]");
			}
		}
	}

	public class VsTestCoverageConverter
	{
		public static void ConvertCoverageFile(string coverageFile, string outputFile = "coverage_report.xml")
		{
			using (CoverageInfo info = CoverageInfo.CreateFromFile(coverageFile, new string[] { }, new string[] { }))
			{
				CoverageDS data = info.BuildDataSet();
				data.WriteXml(outputFile);
			}
		}
	}
}
