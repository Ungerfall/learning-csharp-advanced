using System;
using System.Drawing;
using System.Windows.Forms;

namespace DownloadManager.WindowsFormsApp
{
	public class DownloadTaskView : FlowLayoutPanel
	{
		private readonly DownloadTaskViewModel viewModel;

		public DownloadTaskView(DownloadTaskViewModel viewModel)
		{
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			FlowDirection = FlowDirection.LeftToRight;
			WrapContents = false;
			Height = 50;
			Width = 600;

			var addressInput = new TextBox
			{
				Size = new Size(128, 32),
			};
			addressInput.DataBindings.Add("Enabled", viewModel, nameof(viewModel.CanEditSource));
			addressInput.DataBindings.Add("Text", viewModel, nameof(viewModel.Source));
			var downloadButton = new Button
			{
				Text = "Download",
				AutoSize = true,
			};
			downloadButton.DataBindings.Add("Enabled", viewModel, nameof(viewModel.CanDownloadContentAsync));
			downloadButton.Click += (s, e) => viewModel.DownloadContentAsync();
			var cancelButton = new Button
			{
				Text = "Cancel",
				AutoSize = true,
			};
			cancelButton.DataBindings.Add("Enabled", viewModel, nameof(viewModel.CanCancelDownloading));
			cancelButton.Click += (s, e) => viewModel.CancelDownloading();
			var resultButton = new Button
			{
				Text = "Result",
				AutoSize = true,
			};
			resultButton.DataBindings.Add("Enabled", viewModel, nameof(viewModel.CanShowResult));
			resultButton.Click += (s, e) => viewModel.ShowResult();
			var state = new Label
			{
				Size = new Size(128, 32),
				TextAlign = ContentAlignment.MiddleLeft
			};
			state.DataBindings.Add("Text", viewModel, nameof(viewModel.ProgressState));

			Controls.Add(addressInput);
			Controls.Add(downloadButton);
			Controls.Add(cancelButton);
			Controls.Add(resultButton);
			Controls.Add(state);
		}
	}
}
