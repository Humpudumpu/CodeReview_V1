using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections;
using System.Diagnostics;

using System.Threading.Tasks;

namespace CodeReview
{
	public class FindText
	{
		public void Start(string file, string text, int startChangeSet, int stopChangeSet)
		{
			var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("http://can10tfsprd1:8080/tfs/can10tpc4"));
			var branch = @"$/USCAN/Internal/UScaniPedia/1.0/1.0dev/source";
			var versionControl = tfs.GetService<VersionControlServer>();

			var fileX = branch + @"/Announcements.aspx";

			var teamBranches = tfs.GetService<VersionControlServer>().QueryRootBranchObjects(RecursionType.Full).
				Where(s => !s.Properties.RootItem.IsDeleted);
			SearchInFile("test");
		}

		private void SearchInFile(string item)
		{
		}
	}

}
