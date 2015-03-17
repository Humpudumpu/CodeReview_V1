using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace CodeReview
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private CodeReview codeReview;
		private string incidentNo;
		private const int MAXINCIDENTHISTORY = 10;

		public ObservableCollection<FileObject> FileObjects { get {return fileObjects;}}
		private ObservableCollection<FileObject> fileObjects  = new ObservableCollection<FileObject>();

		public ObservableCollection<string> IncidentNoCollection { get { return incidentNoCollection; } }
		private ObservableCollection<string> incidentNoCollection = new ObservableCollection<string>();

		public ObservableCollection<string> FontFamilyCollection { get { return fontFamilyCollection; } }
		private ObservableCollection<string> fontFamilyCollection = new ObservableCollection<string>();

		public ObservableCollection<string> StatusMessageList { get { return statusMessageList; } }
		private ObservableCollection<string> statusMessageList = new ObservableCollection<string>();

		public Command SetSelectedFont { get { return setSelectedFont; } }
		private Command setSelectedFont;

		public Command SetFontSize { get { return setFontSize; } }
		private Command setFontSize;

		private string fontName;
		public string FontName { get { return fontName; } set { fontName = value; } }

		private int fontSize;
		public int FontSize { get { return fontSize; } set { fontSize = value; } }

		public Command FileDiff { get { return fileDiff; } }
		private Command fileDiff;

		public Command GetIncident { get { return getIncident; } }
		private Command getIncident;

		public bool EnableComboBox { get { return enableComboBox; } private set { enableComboBox = value; } }
		private bool enableComboBox;

		public string IncidentNo { get { return incidentNo; } set {	incidentNo = value;	} }
		public uint IncidentAssociationCount { get { return incidentAssociationCount; } set { incidentAssociationCount = value; } }
		private uint incidentAssociationCount;

		public MainWindowViewModel(CodeReview cr)
		{
			this.codeReview = cr;
			this.codeReview.FileListUpdateEvent += codeReview_FileListUpdateEvent;
			this.codeReview.UpdateStatus += codeReview_UpdateStatus;
			fileDiff = new Command(x => codeReview.GetFileDifference(x));
			getIncident = new Command(x => this.GetIncidentAssociations());
			setSelectedFont = new Command(x => this.SetApplicationFont(x));
			setFontSize = new Command(x => this.SetApplicationFontSize(x));
			fontName = "Courier New";
			fontSize = 12;
			ComboEnabled(true);
			GetSupportedFonts();
		void codeReview_UpdateStatus(object sender, string value)
		{
			StatusMessageList.Add(value);
			FirePropertyChanged("StatusMessageList");
		}

		void SetApplicationFont(object fontName)
		{
			FontName = fontName.ToString();
			FirePropertyChanged("FontName");
		}

		void SetApplicationFontSize(object fontSize)
		{
			FontSize = Int32.Parse(fontSize.ToString());
			FirePropertyChanged("FontSize");
		}

		public void IncreaseFontSize()
		{
			if (FontSize < 25)
			{
				FontSize = FontSize + 1;
				FirePropertyChanged("FontSize");
			}
		}

		public void DecreaseFontSize()
		{
			if (FontSize > 10)
			{
				FontSize = FontSize - 1;
				FirePropertyChanged("FontSize");
			}
		}

		void GetSupportedFonts()
		{
			foreach (FontFamily f in FontFamily.Families)
				fontFamilyCollection.Add(f.Name);
		}

		void GetIncidentAssociations()
		{
			StatusMessageList.Clear();
			uint result;
			if (!UInt32.TryParse(IncidentNo, out result))
				return;
			ComboEnabled(false);
			this.codeReview.GetIncident(Convert.ToUInt32(IncidentNo));
		}

		void ComboEnabled(bool enabled)
		{
			EnableComboBox = enabled;
			FirePropertyChanged("ComboBoxEnable");
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
			if (IncidentNoCollection.Count > MAXINCIDENTHISTORY)
				IncidentNoCollection.Remove(IncidentNoCollection.First().ToString());
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
