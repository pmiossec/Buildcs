#r Ionic.Zip.dll

using Ionic.Zip;
//https://dotnetzip.codeplex.com/ (old version, with binaries)
//https://github.com/haf/DotNetZip.Semverd (new version but without binaries)

public partial class Zip
{
	//Zip some files in a zip file
	public static void ZipFilesInArchive(string zipname, params string[] filePaths)
	{
		BuildHelper.DisplayAndLog("Creation of the zip file '" + zipname + "'...");
		BuildHelper.ContinueOrFail(() => {
			using (ZipFile zip = new ZipFile())
			{
				foreach(var file in filePaths)
					zip.AddFile(file);
				zip.Save(System.IO.Path.GetFileName(zipname));
			}
		});
	}
}
