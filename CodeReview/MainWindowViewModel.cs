using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Data;

namespace CodeReview
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private System.Windows.Forms.Control invoker;
		private CodeReview codeReview;
		private string incidentNo;

		public ObservableCollection<FileObject> FileObjects { get {return fileObjects;}}
		private ObservableCollection<FileObject> fileObjects  = new ObservableCollection<FileObject>();

		public ObservableCollection<string> IncidentNoCollection { get { return incidentNoCollection; } }
		private ObservableCollection<string> incidentNoCollection = new ObservableCollection<string>();

		public Command FileDiff { get { return fileDiff; } }
		private Command fileDiff;

		public Command GetIncident { get { return getIncident; } }
		private Command getIncident;

		public string IncidentNo { get { return incidentNo; } set {	incidentNo = value;	} }
		public MainWindowViewModel(CodeReview cr)
		{
			this.codeReview = cr;
			this.codeReview.FileListUpdateEvent += codeReview_FileListUpdateEvent;
			fileDiff = new Command(x => codeReview.GetFileDifference(x));
			getIncident = new Command(x => codeReview.GetIncident(x.ToString()));
		}



		void codeReview_FileListUpdateEvent(object sender, EventArgs e)
		{
			if (codeReview.FileList.Count == 0)
				ClearIncidentAssociationsList();
			else
				UpdateIncidentAssociationsList();
			IncidentAssociationCount = Convert.ToUInt32(FileObjects.Count);
			FirePropertyChanged("IncidentAssociationCount");
			ComboEnabled(true);
		}

		private void UpdateIncidentAssociationsList()
		{
			foreach (FileObject ob in codeReview.FileList)
				fileObjects.Add(ob);
			if (!IncidentNoCollection.Contains(IncidentNo))
				IncidentNoCollection.Add(IncidentNo);
		}

		private void ClearIncidentAssociationsList()
		{
			fileObjects.Clear();
		}


		public event PropertyChangedEventHandler PropertyChanged;
		public void FirePropertyChanged(string propertyName)
		{
			var handle = PropertyChanged;
			if (handle != null)
				handle(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
