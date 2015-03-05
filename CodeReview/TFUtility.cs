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

			associations.AddRange(RelateTFSAssociations(associations));

			return associations;
		}

		private List<ITeamTrack.Association> RelateTFSAssociations(List<ITeamTrack.Association> associations)
		{
			List<ITeamTrack.Association> tfsWholeIncidentBranchAssociations = new List<ITeamTrack.Association>();

			TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(@"http://can10tfsprd1:8080/tfs/can10tpc4"));
			var service = tfs.GetService<VersionControlServer>();
			List<ITeamTrack.Association> tfIncidentBranchs = new List<ITeamTrack.Association>();
			//getting path of the incident branch
			Regex IncidentBranch = new Regex(@"\$?/Incidents/\d+$");
			try
			{
				//tfIncidentBranchs = associations.Where(p => IncidentBranch.IsMatch(p.file)).Distinct().Select(p => p).ToList();
				foreach (ITeamTrack.Association ass in associations)
				{
					if (IncidentBranch.IsMatch(ass.file))
						tfIncidentBranchs.Add(ass);
				}
			}
			catch
			{
				//this is an incident with no tfs checkin's
				return tfsWholeIncidentBranchAssociations;
			}

			Dictionary<string, List<int>> tfsPerChangesetFileHistory = new Dictionary<string, List<int>>();
			ChangesetMerge[] mergedChangeSet = null;
			//#68103
			/*
			 * $/USCAN/CustomSet/Tradeshow/5.2/Incidents/68103
			 * $/USCAN/Product/5.2/Incidents/68103
			 * 
			 * */
			foreach (ITeamTrack.Association tfAssociation in tfIncidentBranchs)
			{
				//Now = > $/USCAN/CustomSet/Tradeshow/5.2/Incidents/68103
				//Getting path of the dev branch
				ItemSpec[] incidentBranch = new ItemSpec[] { new ItemSpec(tfAssociation.file, RecursionType.None) };
				string releasePath = tfAssociation.file.Substring(0, tfAssociation.file.IndexOf("/Incidents"));
				List<string>output = RunTF<string>(String.Format("dir {0} /version:T", releasePath), true).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
				string devSuffix = "dev";
				releasePath = releasePath + output.SingleOrDefault<string>(x => x.Contains(devSuffix)).Replace("$","/");
				string tfDevBranchPath = releasePath;

				List<ITeamTrack.Association> tfsIncidentBranchAssociations = new List<ITeamTrack.Association>();

				List<Changeset> history = service.QueryHistory(tfAssociation.file, RecursionType.Full).ToList<Changeset>();
				//get the changeset ID where the main branch was branched to the incident branch
				mergedChangeSet = service.QueryMerges(new ItemSpec(tfDevBranchPath, RecursionType.None), VersionSpec.Latest, new ItemSpec(tfAssociation.file, RecursionType.None), VersionSpec.Latest, null, null);
				//Now => Changeset all in the incident folder -> branch : 68103
				foreach (Changeset cs in history)
				{
					if (cs.ChangesetId == mergedChangeSet[0].TargetChangeset.ChangesetId)
						break;

					var changeSet = service.GetChangeset(cs.ChangesetId);
					//Now => Each file in one single changeset
					Change[] changes = changeSet.Changes;
					string changeSetComments = cs.Comment.TrimEnd();
					string changeSetAuthor = cs.CommitterDisplayName;
					foreach (Change change in changes)
					{
						ITeamTrack.Association association = new ITeamTrack.Association();
						association.file = change.Item.ServerItem;
						association.author = changeSetAuthor;
						association.logMessage = changeSetComments;

						//One file can be associated to many changesets. So. Get all the changeset no. the file is attached to.
						//Now when you open those files and those changesets, the file change will be present. This is useful to take the differences.
						if (!tfsPerChangesetFileHistory.Keys.Contains(association.file))
							tfsPerChangesetFileHistory.Add(association.file, new List<int>());
						List<int> eachFileChangesetList;
						tfsPerChangesetFileHistory.TryGetValue(association.file, out eachFileChangesetList);
						eachFileChangesetList.Add(change.Item.ChangesetId);

						tfsIncidentBranchAssociations.Add(association);
					}

				}

				foreach (KeyValuePair<string, List<int>> x in tfsPerChangesetFileHistory)
					x.Value.Sort();

				//Have list of changesets a file is involved in. Now need to stitch all the changesets together.
				for (int i = 0; i < tfsIncidentBranchAssociations.Count; i++)
				{
					List<int> fileChangesetList;
					tfsPerChangesetFileHistory.TryGetValue(tfsIncidentBranchAssociations[i].file, out fileChangesetList);
					if (fileChangesetList.Count != 0)
					{
						tfsIncidentBranchAssociations[i].checkInRevision = fileChangesetList.Last().ToString();
						fileChangesetList.Remove(fileChangesetList.Last());
					}

					//When the file change set list means we have not reached the changeset where the branching had began
					if (fileChangesetList.Count > 0)
						tfsIncidentBranchAssociations[i].checkOutRevision = fileChangesetList.Last().ToString();
					else
						//When we reach the end, blindly attached the changeset number from where the branch was created.
						tfsIncidentBranchAssociations[i].checkOutRevision = mergedChangeSet[0].TargetChangeset.ChangesetId.ToString();
				}
				tfsWholeIncidentBranchAssociations.AddRange(tfsIncidentBranchAssociations);
			}

			return tfsWholeIncidentBranchAssociations;
		}

		public T RunTF<T>(string arguments, bool redirectOutput = false, int waitMinutes = 2)
		{
			string standardOutput = String.Empty;
			Process tf = new Process();
			tf.StartInfo.FileName = filename;
			tf.StartInfo.Arguments = arguments;
			tf.StartInfo.RedirectStandardOutput = redirectOutput;
			tf.StartInfo.CreateNoWindow = true;
			tf.StartInfo.UseShellExecute = false;
			tf.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			try
			{
				tf.Start();
				if (tf.StartInfo.RedirectStandardOutput)
					standardOutput = tf.StandardOutput.ReadToEnd();

				if (waitMinutes > 0)
				{
					tf.WaitForExit(waitMinutes * 60 * 1000);
					return tf.StartInfo.RedirectStandardOutput ? (T)(object)standardOutput : (T)(object)tf.ExitCode;
				}
				else
					return (T)(object)0;
			}
			finally { tf.Close(); }
		}
	}
}
