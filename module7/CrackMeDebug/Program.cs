using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CrackMeDebug
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.Run(new CrackMe.Form1()); 
		}
	}
}
