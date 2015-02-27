﻿using System;
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
	public class MainWindowViewModel
	{
		private System.Windows.Forms.Control invoker;
		private CodeReview codeReview;
		public ObservableCollection<FileObject> FileObjects { get {return fileObjects;}}
		private ObservableCollection<FileObject> fileObjects  = new ObservableCollection<FileObject>();

		public Command FileDiff { get { return fileDiff; } }
		private Command fileDiff;

		public Command GetIncident { get { return getIncident; } }
		private Command getIncident;

		public MainWindowViewModel(CodeReview cr, Control invoker = null)
		{
			this.codeReview = cr;
			this.invoker = invoker;
			this.codeReview.PropertyChanged += codeReview_PropertyChanged;
			fileDiff = new Command(x => codeReview.GetFileDifference(x));
			getIncident = new Command(x => codeReview.GetIncident(x.ToString()));
		}

		private void codeReview_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "FileList")
			{
				foreach (FileObject ob in this.codeReview.FileList)
					fileObjects.Add(ob);
			}
			else if (e.PropertyName == "FileList_Clear")
			{
				fileObjects.Clear();
			}
		}
	}
}
