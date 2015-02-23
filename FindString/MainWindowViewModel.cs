using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace CodeReview
{
	public class MainWindowViewModel
	{
		private System.Windows.Forms.Control invoker;
		private CodeReview codeReview;
		public ObservableCollection<FileObject> FileObjects { get {return fileObjects;}}
		private ObservableCollection<FileObject> fileObjects  = new ObservableCollection<FileObject>();

		public MainWindowViewModel(CodeReview cr, System.Windows.Forms.Control invoker = null)
		{
			this.codeReview = cr;
			this.invoker = invoker;
			this.codeReview.OnFileListUpdate +=codeReview_OnFileListUpdate;
		}

		void codeReview_OnFileListUpdate(object sender, FileListUpdateArgs e)
		{
 			List<FileObject> tempList = e.files;
			foreach(FileObject ob in tempList)
				fileObjects.Add(ob);
		}
	}
}
