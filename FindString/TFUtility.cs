using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PVCSTools;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;

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

			List<ITeamTrack.Association> associations = teamTrack.GetAssociations(incidentNo);
			//SBM does not make a one to one association of what was checked out and what was checkin. It only says what was checkedout.
			//For code review to work - we need a one to one association = i.e what was checkout from devbranch, and what was finally merged to the devbranch.

			RelateTFSAssociations(associations);

			return associations;
		}

		private void RelateTFSAssociations(List<ITeamTrack.Association> associations)
		{
			//Collect all incidents that have been checked-in to TFS
			List<ITeamTrack.Association> tfsAssociations = associations.TakeWhile(x => x.file.Contains(@"$/")).ToList<ITeamTrack.Association>();

			//changes done on Incident branch. 
			//What I intend?
			//The changes done in incident branch are not displayed file by file. In SBM it is shown as:
			/*
			$/USCAN/CustomSet/TradeshowGenesis/5.2/Incidents/68103 Modify File Association  Delete File Association
 	 		Revision 1551 Checked In by (None) 10/02/2015 06:22:48 PM
 	 	    Branched from $/USCAN/CustomSet/TradeshowGenesis/5.2/5.2dev */
			//Now if the change is done in just one file - It is still shown like above.
			//So I cannot create one to one correspondance between the checkedout file and checkedin file because the filename is not mentioned when
			//creating the Incident branch.
			//So I have to find the file from the incident beanch display that changeset number and then use the file to find out the checked in changeset number in the dev branch.

			List<ITeamTrack.Association> tfsIncidentBranchAssociations = tfsAssociations.TakeWhile(x => x.file.Contains(@"dev")).ToList<ITeamTrack.Association>();

			TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(@"http://can10tfsprd1:8080/tfs/can10tpc4"));
			var service = tfs.GetService<VersionControlServer>();
			List<Changeset>history = service.QueryHistory("$/USCAN/Product/5.0SON/Incidents/72382", RecursionType.Full).ToList<Changeset>();
			string item = "$/USCAN/Product/5.0SON/Incidents/72382";
			ChangesetMerge[] mergedChangeSet = service.QueryMerges(new ItemSpec("$/USCAN/Product/5.0SON/5.0dev", RecursionType.None), VersionSpec.Latest, new ItemSpec(item, RecursionType.None), VersionSpec.Latest, null, null);

			foreach(Changeset cs in history)
			{
				if (cs.ChangesetId == mergedChangeSet[0].TargetChangeset.ChangesetId)
					return;

				var changeSet = service.GetChangeset(cs.ChangesetId);
				Change[] changes = changeSet.Changes;
				string changeSetComments = String.Empty;
				string changeSetAuthor = cs.Committer;
				foreach (Change change in changes)
				{
					ITeamTrack.Association association = new ITeamTrack.Association();
					association.file = change.Item.ServerItem;
					association.checkOutRevision = change.Item.ChangesetId.ToString();
					association.checkOutRevision = change.Item.ChangesetId.ToString();
					association.author = changeSetAuthor;
					association.logMessage = changeSetComments;
					associations.Add(association);
				}
			}
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
