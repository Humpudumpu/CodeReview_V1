using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using PVCSTools;


namespace CodeReview
{
	public class CodeReview
	{
		public delegate void FileListUpdateHandler(object sender, FileListUpdateArgs e);
		public event FileListUpdateHandler OnFileListUpdate; 

		private List<FileObject> fileList;
		private TFUtility tf;
		
		public CodeReview()
		{
			fileList = new List<FileObject>();
			tf = new TFUtility();
		}

		public void GetIncident(uint incident)
		{
			PopulateFileObjects(tf.GetAssociations(incident));
			if (OnFileListUpdate == null)
				return;
			FileListUpdateArgs args = new FileListUpdateArgs(fileList);
			OnFileListUpdate(this, args);
		}

		private void Add(FileObject f)
		{
			fileList.Add(f);
		}

		private void PopulateFileObjects(List<ITeamTrack.Association> associations)
		{
			if (associations.Count == 0)
				return;

			foreach(ITeamTrack.Association association in associations)
			{
				fileList.Add(new FileObject(association.file, "", association.logMessage, association.checkInRevision, association.checkOutRevision, association.author));
			}
		}
	}




	public class FileListUpdateArgs : EventArgs
	{
		public List<FileObject> files { get; private set; }
		public FileListUpdateArgs(List<FileObject> list)
		{
			files = list;
		}
	}
}
