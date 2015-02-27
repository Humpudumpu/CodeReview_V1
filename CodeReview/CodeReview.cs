using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using PVCSTools;
using System.Collections;



namespace CodeReview
{
	public class CodeReview : INotifyPropertyChanged
	{
		private List<FileObject> fileList;
		private TFUtility tf;

		public event PropertyChangedEventHandler PropertyChanged;

		public void FirePropertyChanged(string propertyName)
		{
			var handle = PropertyChanged;
			if (handle != null)
				handle(this, new PropertyChangedEventArgs(propertyName));
		}
		
		public CodeReview()
		{
			fileList = new List<FileObject>();
			tf = new TFUtility();
		}

		public List<FileObject> FileList { get { return fileList; } }

		public void GetIncident(string incidentNo)
		{

			uint incident = 0;

			//Here we can update a status box in the gui
			//assuming TextBox sends data as string
			if (!uint.TryParse(incidentNo, out incident))
				return;

			ClearFileList();
			PopulateFileObjects(tf.GetAssociations(incident));
		}

		private void ClearFileList()
		{
			fileList.Clear();
			FirePropertyChanged("FileList_Clear");
		}

		private void PopulateFileObjects(List<ITeamTrack.Association> associations)
		{
			if (associations.Count == 0)
				return;

			foreach(ITeamTrack.Association association in associations)
			{
				fileList.Add(new FileObject(association.file, "", association.logMessage, association.checkOutRevision, association.checkInRevision, association.author));
			}
			FirePropertyChanged("FileList");
		}

		public void GetFileDifference(Object ob)
		{
			List<FileObject> associations = ((IEnumerable)ob).Cast<FileObject>().ToList();
			string testing = String.Empty;
			string argument = String.Empty;
			if (associations.Count == 0)
				argument = String.Format("difference {0} /version:C{1}~C{2} /format:visual", associations[0].Filename, associations[0].CheckOutChangeSet, associations[0].CheckInChangeSet);
			else
				testing = "multiple";
			//tf.RunTF<int>(argument, false);
		}
	}
}
