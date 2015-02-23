using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CodeReview
{
	public class TFUtility
	{
		string filename = String.Empty;

		public TFUtility()
		{
			filename = Path.Combine(Environment.GetEnvironmentVariable("VS110COMNTOOLS"), "tf.exe");
		}

		public int GetChangesFileList(int incidentNo)
		{
			return 0;
		}

		public T RunTF<T>(string arguments, bool redirectOutput = false)
		{
			string output = String.Empty;
			ProcessStartInfo tfInfo = new ProcessStartInfo();
			tfInfo.FileName = filename;
			tfInfo.Arguments = arguments;
			tfInfo.RedirectStandardOutput = redirectOutput;
			tfInfo.CreateNoWindow = false;
			tfInfo.WindowStyle = ProcessWindowStyle.Hidden;
			Process tf = new Process();
			try
			{
				tf.StartInfo = tfInfo;
				tf.Start();
				if (redirectOutput)
					output = tf.StandardOutput.ReadToEnd();

				return redirectOutput ? (T)(object)output : (T)(object)tf.ExitCode;
			}
			finally { tf.Close(); }
		}
	}
}
