using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DocumentsHub.Monitoring.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Status of Documents Joiner clients:");
				var path = DocumentsJoinerClientStatusReceiverWorker.ClientStatusFileFullPath;
				if (!File.Exists(path))
				{
					var f = File.Create(path);
					f.Close();
					f.Dispose();
				}

				using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var sr = new StreamReader(file, Encoding.UTF8))
				{
					while (true)
					{
						string line = sr.ReadLine();
						if (line == null)
						{
							Thread.Sleep(3 * 1000);
							continue;
						}

						Console.WriteLine(line);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
	}
}
