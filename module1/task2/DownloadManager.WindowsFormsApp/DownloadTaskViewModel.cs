using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DownloadManager.WindowsFormsApp
{
	public class DownloadTaskViewModel : INotifyPropertyChanged, IDisposable
	{
		private string downloadResult = string.Empty;
		private bool canDownloadContentAsync = true;
		private bool canShowResult = false;
		private bool canCancelDownloading = false;
		private bool canEditSource = true;
		private string progressState = "input";

		private readonly IDownloadTask downloadTask;
		private readonly Action<string> viewResultsAction;

		public DownloadTaskViewModel(IDownloadTask downloadTask, Action<string> viewResultsAction)
		{
			if (downloadTask == null) throw new System.ArgumentNullException(nameof(downloadTask));
			if (viewResultsAction == null) throw new System.ArgumentNullException(nameof(viewResultsAction));

			this.downloadTask = downloadTask;
			this.viewResultsAction = viewResultsAction;
		}

		public async void DownloadContentAsync()
		{
			if (!CanDownloadContentAsync)
				return;

			try
			{
				ProgressState = "downloading";
				CanDownloadContentAsync = false;
				CanCancelDownloading = true;
				CanEditSource = false;
				CanShowResult = false;

				downloadResult = await downloadTask.DownloadAsync(Source);
				ProgressState = "success";
				CanDownloadContentAsync = true;
				CanCancelDownloading = false;
				CanShowResult = true;
				CanEditSource = true;
			}
			catch (Exception ex)
			{
				downloadResult = ex.Message;
				CanDownloadContentAsync = true;
				CanCancelDownloading = false;
				CanEditSource = true;
				CanShowResult = true;
				ProgressState = "fault";
			}
		}

		public void ShowResult()
		{
			if (!CanShowResult)
				return;

			viewResultsAction(downloadResult);
		}

		public void CancelDownloading()
		{
			if (!CanCancelDownloading)
				return;

			downloadTask.Cancel();
			ProgressState = "cancelled";
			CanDownloadContentAsync = true;
			CanCancelDownloading = false;
			CanShowResult = false;
			CanEditSource = true;
		}

		public string Source { get; set; }

		public bool CanDownloadContentAsync
		{
			get => canDownloadContentAsync; private set
			{
				canDownloadContentAsync = value;
				OnPropertyChanged();
			}
		}

		public bool CanShowResult
		{
			get => canShowResult; private set
			{
				canShowResult = value;
				OnPropertyChanged();
			}
		}

		public bool CanCancelDownloading
		{
			get => canCancelDownloading; private set
			{
				canCancelDownloading = value;
				OnPropertyChanged();
			}
		}

		public bool CanEditSource
		{
			get => canEditSource; private set
			{
				canEditSource = value;
				OnPropertyChanged();
			}
		}

		public string ProgressState
		{
			get => progressState; private set
			{
				progressState = value;
				OnPropertyChanged();
			}
		}

		public void Dispose()
		{
			downloadTask?.Dispose();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
