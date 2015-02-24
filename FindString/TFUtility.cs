using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PVCSTools;

namespace CodeReview
{
	public class TFUtility
	{
		string filename = String.Empty;
		const string user = "CAN10TFSBridgeSvc";
		const string password = "Canadatfs.2012";
		

		//PCVSTools members
		TeamTrack teamTrack;
		bool LoggedIn { get; set; }

		public TFUtility()
		{
			filename = Path.Combine(Environment.GetEnvironmentVariable("VS110COMNTOOLS"), "tf.exe");
			teamTrack = new TeamTrack();
			LoggedIn = false;
		}

		public List<ITeamTrack.Association> GetAssociations(uint incidentNo)
		{
			
			if (!LoggedIn)
				LoggedIn = teamTrack.Login(user, password);

			return teamTrack.GetAssociations(incidentNo);
		}

		public T RunTF<T>(string arguments, bool redirectOutput = false, int waitMinutes = 2)
		{
			string standardOutput = String.Empty;
			ProcessStartInfo tfInfo = new ProcessStartInfo();
			tfInfo.FileName = filename;
			tfInfo.Arguments = arguments;
			tfInfo.RedirectStandardOutput = redirectOutput;
			tfInfo.CreateNoWindow = false;
			tfInfo.WindowStyle = ProcessWindowStyle.Hidden;

			Process process = new Process();
			try
			{
				process.StartInfo = tfInfo;
				process.Start();
				if (tfInfo.RedirectStandardOutput)
					standardOutput = process.StandardOutput.ReadToEnd();

				process.WaitForExit(waitMinutes * 60 * 1000);
				return tfInfo.RedirectStandardOutput ? (T)(object)standardOutput : (T)(object)process.ExitCode;
			}
			finally { process.Close(); }
		}
	}
}
