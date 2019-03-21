using System.Windows.Forms;

namespace DownloadManager.WindowsFormsApp
{
	public partial class TextViewerForm : Form
	{
		public TextViewerForm(string text)
		{
			InitializeComponent();
			textBox1.Text = text;
		}
	}
}
