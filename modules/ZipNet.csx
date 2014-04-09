using System.IO.Compression;
using System.IO.Compression.ZipArchive;

//Need .Net Framework 4.5!
public partial class Zip
{
	//Zip some files in a zip file
	public static void ZipFilesInArchive(string zipname, params string[] filePaths)
	{
		BuildHelper.DisplayAndLog("Creation of the zip file '" + zipname + "'...");
		BuildHelper.ContinueOrFail(() => {
			using (ZipArchive newFile = ZipFile.Open(zipName, ZipArchiveMode.Create))
			{
				foreach(var file in filePaths)
					newFile.CreateEntryFromFile(file, System.IO.Path.GetFileName(file));
			}
		});
	}
}
