using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
			filename = Path.Combine(Environment.GetEnvironmentVariable("VS110COMNTOOLS").Replace("Tools","IDE"), "tf.exe");
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
			//List<ITeamTrack.Association> tfsAssociations = associations.TakeWhile(x => x.file.Contains(@"$/")).ToList<ITeamTrack.Association>();

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
			//Regex incidentRegx = new Regex(@"\k<Incidents>");
			//foreach(ITeamTrack.Association x in tfsAssociations)
			//{
			//	Match match =incidentRegx.Match(x.file);
			//	if (match.Success)
			//		Console.WriteLine("file matched" + x.file);
			//}
			
		//	List<ITeamTrack.Association> tfsIncidentBranchAssociationsPath = tfsAssociations.TakeWhile(x => x.file.Contains(@"dev")).ToList<ITeamTrack.Association>();

			TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(@"http://can10tfsprd1:8080/tfs/can10tpc4"));
			var service = tfs.GetService<VersionControlServer>();
			ITeamTrack.Association tfAssociation;
			//getting path of the incident branch
			try
			{
				tfAssociation = associations.Single(p => p.file.Contains("Incidents"));
			}
			catch
			{
				//this is an incident with no tfs checkin's
				return;
			}
			
			//Getting path of the dev branch
			ItemSpec[] ais = new ItemSpec[] { new ItemSpec(tfAssociation.file, RecursionType.None) };
			BranchHistoryTreeItem[][] branchHistoryTree = service.GetBranchHistory( ais , VersionSpec.Latest);
			string tfDevBranchPath = branchHistoryTree[0][0].Relative.BranchToItem.ServerItem;

			List<Changeset>history = service.QueryHistory(tfAssociation.file, RecursionType.Full).ToList<Changeset>();
			ChangesetMerge[] mergedChangeSet = service.QueryMerges(new ItemSpec(tfDevBranchPath, RecursionType.None), VersionSpec.Latest, new ItemSpec(tfAssociation.file, RecursionType.None), VersionSpec.Latest, null, null);

			List<ITeamTrack.Association> tfsIncidentBranchAssociations = new List<ITeamTrack.Association>();
			Dictionary<string, List<int>> tfsPerChangesetFileHistory = new Dictionary<string, List<int>>();

			foreach(Changeset cs in history)
			{
				if (cs.ChangesetId == mergedChangeSet[0].TargetChangeset.ChangesetId)
					break;

				var changeSet = service.GetChangeset(cs.ChangesetId);
				Change[] changes = changeSet.Changes;
				string changeSetComments = cs.Comment.TrimEnd();
				string changeSetAuthor = cs.CommitterDisplayName;
				foreach (Change change in changes)
				{
					ITeamTrack.Association association = new ITeamTrack.Association();
					association.file = change.Item.ServerItem;
					//association.checkOutRevision = tfsAssociations.Count == 0 ? mergedChangeSet[0].TargetChangeset.ChangesetId.ToString() : "test";
					//association.checkInRevision = change.Item.ChangesetId.ToString();
					association.author = changeSetAuthor;
					association.logMessage = changeSetComments;

					if (!tfsPerChangesetFileHistory.Keys.Contains(association.file))
						tfsPerChangesetFileHistory.Add(association.file, new List<int>());
					List<int> eachFileChangesetList;
					tfsPerChangesetFileHistory.TryGetValue(association.file, out eachFileChangesetList);
					eachFileChangesetList.Add(change.Item.ChangesetId);

					tfsIncidentBranchAssociations.Add(association);
				}
			}

			foreach (KeyValuePair<string, List<int>> x in tfsPerChangesetFileHistory)
			{
				x.Value.Sort();
			}

			//Have list of changesets a file is involved in. Now need to stitch all the changesets together.
			for(int i = 0; i < tfsIncidentBranchAssociations.Count; i++)
			{
				List<int> fileChangesetList;
				tfsPerChangesetFileHistory.TryGetValue(tfsIncidentBranchAssociations[i].file, out fileChangesetList);
				if (fileChangesetList.Count != 0)
				{
					tfsIncidentBranchAssociations[i].checkInRevision = fileChangesetList.Last().ToString();
					fileChangesetList.Remove(fileChangesetList.Last());
				}

				if (fileChangesetList.Count > 0)
					tfsIncidentBranchAssociations[i].checkOutRevision = fileChangesetList.Last().ToString();
				else
					tfsIncidentBranchAssociations[i].checkOutRevision = mergedChangeSet[0].TargetChangeset.ChangesetId.ToString();

				associations.Add(tfsIncidentBranchAssociations[i]);
			}
		}

		public T RunTF<T>(string arguments, bool redirectOutput = false, int waitMinutes = 2)
		{
			string standardOutput = String.Empty;
			Process tf = new Process();
			tf.StartInfo.FileName = filename;
			tf.StartInfo.Arguments = arguments;
			tf.StartInfo.RedirectStandardOutput = redirectOutput;
			tf.StartInfo.CreateNoWindow = false;
			tf.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			try
			{
				tf.Start();
				if (tf.StartInfo.RedirectStandardOutput)
					standardOutput = tf.StandardOutput.ReadToEnd();

				tf.WaitForExit(waitMinutes * 60 * 1000);
				if (tf.ExitCode != 0)
				{
					tf.StartInfo.Arguments = tf.StartInfo.Arguments + " > tfErrorLog.txt 2>&1";
					tf.Start();
					tf.WaitForExit(waitMinutes * 60 * 1000);
				}
				return tf.StartInfo.RedirectStandardOutput ? (T)(object)standardOutput : (T)(object)tf.ExitCode;
			}
			finally { tf.Close(); }
		}
	}
}
