using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace OctokitWrapper
{
	/// <summary>
	/// Create a release on github.com.
	/// </summary>
	/// <example>
	/// <code><![CDATA[
	/// <ItemGroup>
	/// <ReleaseFiles Include="MyAwesomeProject-v0.1.0.zip" />
	/// <ReleaseFiles Include="MyAwesomeReleaseNotes.md">
	/// <ContentType>text/plain</ContentType>
	/// </ReleaseFiles>
	/// </ItemGroup>
	/// <ItemGroup>
	/// <ReleaseNotesFile Include="MyAwesomeReleaseNotes.md" />
	/// </ItemGroup>
	/// <Target Name="Release">
	/// <CreateRelease Repository="owner/repo" OauthToken="$(GitHubAuthToken)" TagName="v0.1.0" Files="@(ReleaseFiles)" ReleaseNotesFile="$(ReleaseNotesFile)" />
	/// </Target>
	/// ]]></code>
	/// </example>
	public class OctokitWrapper
	{
		//Output
		public string[] UploadedAssets { get; private set; }

		//Output
		public int IdRelease { get; private set; }

		private string Owner { get { return Repository.Split('/')[0]; } }

		private string RepositoryName { get { return Repository.Split('/')[1]; } }

		private ICredentialStore CredentialStore { get { return new InPlaceCredentialStore(OauthToken); } }

		class InPlaceCredentialStore : ICredentialStore
		{
			string _token;
			public InPlaceCredentialStore(string token)
			{
				_token = token;
			}

			public async Task<Credentials> GetCredentials()
			{
				return new Credentials(_token);
			}
		}
		//[Required]
		public string Repository { get; set; }

		//[Required]
		public string OauthToken { get; set; }

		//[Required]
		public string TagName { get; set; }

		public string ReleaseNotesFile { get; set; }

		public bool Release(UploadFile[] files)
		{
			var client = new GitHubClient(new Octokit.ProductHeaderValue("GitTfsTasks"), CredentialStore).Release;
			var release = client.CreateRelease(Owner, RepositoryName, BuildReleaseData()).Result;
			IdRelease = release.Id;
			//BuildHelper.DisplayAndLog(string.Format("Created Release {0} at {1}", release.TagName, release.HtmlUrl));
			if (files != null && files.Length != 0)
			{
				UploadedAssets = UploadAll(client, release, files);
				//foreach (var item in UploadedAssets) BuildHelper.DisplayAndLog(string.Format("Uploaded {0}", item));
			}

			return true;
		}

		private string[] UploadAll(IReleasesClient client, Release release, IEnumerable<UploadFile> items)
		{
			return items.Select(item =>
				{
					//BuildHelper.DisplayAndLog(string.Format("Uploading {0}...", item));
					return Upload(client, release, item);
				}).ToArray();
		}

		private string Upload(IReleasesClient client, Release release, UploadFile sourceItem)
		{
			var uploadedAsset = client.UploadAsset(release, BuildAssetUpload(sourceItem));
			return TaskItemFor(release, uploadedAsset);
		}

		private ReleaseUpdate BuildReleaseData()
		{
			var release = new ReleaseUpdate(TagName);
			if (ReleaseNotesFile != null)
			{
				release.Body = File.ReadAllText(ReleaseNotesFile);
			}

			return release;
		}

		private ReleaseAssetUpload BuildAssetUpload(UploadFile item)
		{
			return new ReleaseAssetUpload
				{
					ContentType = item.ContentType ?? "application/octet-stream",
					FileName = Path.GetFileName(item.Path),
					RawData = File.OpenRead(item.Path)
				};
		}

		private string TaskItemFor(Release release, Task<ReleaseAsset> asset)
		{
			string item;
			// I don't think there's a way, via the API, to get something like this:
			// https://github.com/git-tfs/msbuild-tasks/releases/download/v0.0.9/GitTfsTasks-0.0.9.zip
			item = "https://github.com/" + Repository + "/releases/download/" + TagName + "/" + asset.Result.Name;
			//item.MaybeSetMetadata("ContentType", asset.ContentType);
			//item.MaybeSetMetadata("Id", asset.Id.ToString());
			//item.MaybeSetMetadata("Label", asset.Label);
			//item.MaybeSetMetadata("Name", asset.Name);
			//item.MaybeSetMetadata("State", asset.State);
			return item;
		}
	}
}