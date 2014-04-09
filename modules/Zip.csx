#r Ionic.Zip.dll

using Ionic.Zip;

public class ZipLib
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
