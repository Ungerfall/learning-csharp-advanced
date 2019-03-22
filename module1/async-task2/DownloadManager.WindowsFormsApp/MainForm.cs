using System;
using System.Windows.Forms;

namespace DownloadManager.WindowsFormsApp
{
	public partial class MainForm : Form
	{
		private const int MAX_TASKS = 5;

		private DownloadManager downloadManager = new DownloadManager();
		private int taskCount = 0;

		public MainForm()
		{
			InitializeComponent();
		}

		private void CreateDownloadTaskView()
		{
			var viewModel = new DownloadTaskViewModel(downloadManager.CreateNewDownloadTask(), ShowResultsInMessageBox);
			flowLayoutPanel.Controls.Add(new DownloadTaskView(viewModel));
		}

		private void ShowResultsInMessageBox(string results)
		{
			using (var viewer = new TextViewerForm(results))
			{
				viewer.ShowDialog(this);
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (taskCount >= MAX_TASKS)
				return;
			CreateDownloadTaskView();
			taskCount++;
		}
	}
}
