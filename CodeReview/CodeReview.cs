using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using PVCSTools;

namespace CodeReview
{
	public class CodeReview
	{

		private List<FileObject> fileList;
		private TFUtility tf;

		public event EventHandler FileListUpdateEvent;
		protected virtual void OnFileListUpdatedEvent(EventArgs e)
		{
			var handler = FileListUpdateEvent;
			if (handler != null)
				handler(this, e);
		}

		public CodeReview()
		{
			fileList = new List<FileObject>();
			tf = new TFUtility();
		}

		public List<FileObject> FileList { get { return fileList; } }

		public void GetIncident(uint incidentNo)
		{
			ClearFileList();
			//If the incident was found and there are associations, then the property 'FileList' will be posted;
			PopulateFileObjects(tf.GetAssociations(incidentNo));
		}

		private void ClearFileList()
		{
			fileList.Clear();
			OnFileListUpdatedEvent(EventArgs.Empty);
		}

		private void PopulateFileObjects(List<ITeamTrack.Association> associations)
		{
			if (associations.Count == 0)
				return;

			foreach(ITeamTrack.Association association in associations)
			{
				fileList.Add(new FileObject(association.file, "", association.logMessage, association.checkOutRevision, association.checkInRevision, association.author));
			}
			OnFileListUpdatedEvent(EventArgs.Empty);
		}

		public void GetFileDifference(object ob)
		{
			List<FileObject> associations = ((IEnumerable)ob).Cast<FileObject>().ToList();
			string argument = String.Empty;
			FileObject association = new FileObject();
			if (associations.Count > 1)
			{
				
				FileObject minCheckedOutFile = associations.Aggregate((c, d) => Convert.ToInt32(c.CheckOutChangeSet) < Convert.ToInt32(d.CheckOutChangeSet) ? c : d);
				FileObject maxCheckedInFile = associations.Aggregate((c, d) => Convert.ToInt32(c.CheckInChangeSet) > Convert.ToInt32(d.CheckInChangeSet) ? c : d);
				if (minCheckedOutFile.Filename != maxCheckedInFile.Filename)
				{
					//status message;
					Console.WriteLine("Choose same file");
					return;
				}
				association.Filename = minCheckedOutFile.Filename;
				association.CheckOutChangeSet = minCheckedOutFile.CheckOutChangeSet;
				association.CheckInChangeSet = maxCheckedInFile.CheckInChangeSet;
			}
			else
			{
				association.CheckOutChangeSet = associations[0].CheckOutChangeSet;
				association.CheckInChangeSet = associations[0].CheckInChangeSet;
				association.Filename = associations[0].Filename;
			}

			argument = String.Format("difference {0} /version:C{1}~C{2} /format:visual", association.Filename, association.CheckOutChangeSet, association.CheckInChangeSet);
			tf.RunTF<int>(argument, false, -1);
		}
	}
}
