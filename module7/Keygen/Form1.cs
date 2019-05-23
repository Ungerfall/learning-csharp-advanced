using System;
using System.Linq;
using System.Windows.Forms;

namespace Keygen
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private static byte[] dateBytes;

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			var addressBytes = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
				.FirstOrDefault()
				?.GetPhysicalAddress()
				.GetAddressBytes();
			dateBytes = BitConverter.GetBytes(DateTime.Now.Date.ToBinary());
			int[] source = addressBytes
				.Select(new Func<byte, int, int>(eval_a))
				.Select(eval_a)
				.ToArray();

			textBox1.Text = string.Join("-", source);
		}

		private int eval_a(int arg)
		{
			if (arg <= 999)
			{
				return arg * 10;
			}
	
			return arg;
		}

		private int eval_a(byte addressByte, int index)
		{
			return addressByte ^ dateBytes[index];
		}
	}
}
