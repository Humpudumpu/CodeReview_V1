﻿using System;
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
