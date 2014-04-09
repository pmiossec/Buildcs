using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace GitHubWrapper
{
	/// <summary>
	/// Create a release on github.com.
	/// </summary>
	public class GitHubWrapper
	{
		public string[] UploadedAssets { get; private set; }

		public int IdRelease { get; private set; }

		private string Owner { get { return _repository.Split('/')[0]; } }

		private string RepositoryName { get { return _repository.Split('/')[1]; } }

		private ICredentialStore CredentialStore { get { return new InPlaceCredentialStore(_oauthToken); } }

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

		private string _repository;

		private string _oauthToken;

		private string _tagName;

		private string _releaseNotesFile;

		public bool Release(string repository, string oauthToken, string tagName, UploadFile[] files, string releaseNotesFile)
		{
			_repository = repository;
			_oauthToken = oauthToken;
			_tagName = tagName;
			_releaseNotesFile = releaseNotesFile;

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
			var release = new ReleaseUpdate(_tagName);
			if (_releaseNotesFile != null)
			{
				release.Body = File.ReadAllText(_releaseNotesFile);
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
			return "https://github.com/" + _repository + "/releases/download/" + _tagName + "/" + asset.Result.Name;
			//item.MaybeSetMetadata("ContentType", asset.ContentType);
			//item.MaybeSetMetadata("Id", asset.Id.ToString());
			//item.MaybeSetMetadata("Label", asset.Label);
			//item.MaybeSetMetadata("Name", asset.Name);
			//item.MaybeSetMetadata("State", asset.State);
		}
	}
}