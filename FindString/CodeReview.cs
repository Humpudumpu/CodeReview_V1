using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace FindString
{
	public class CodeReview
	{
		public delegate void FileListUpdateHandler(object sender, FileListUpdateArgs e);
		public event FileListUpdateHandler OnFileListUpdate; 

		//Test code
		public List<FileObject> fileList;
		public CodeReview()
		{
			fileList = new List<FileObject>();
		}

		public void GetIncident(int incident)
		{
			fileList.Add(new FileObject("filename1", "devBranch1", "comments1"));
			fileList.Add(new FileObject("filename2", "devBranch2", "comments2"));
			fileList.Add(new FileObject("filename3", "devBranch3", "comments3"));
			fileList.Add(new FileObject("filename4", "devBranch4", "comments4"));
			fileList.Add(new FileObject("filename5", "devBranch5", "comments5"));
			fileList.Add(new FileObject("filename6", "devBranch6", "comments6"));
			fileList.Add(new FileObject("filename6", "devBranch7", "comments7"));
			if (OnFileListUpdate == null)
				return;
			FileListUpdateArgs args = new FileListUpdateArgs(fileList);
			OnFileListUpdate(this, args);
		}

		private void Add(FileObject f)
		{
			fileList.Add(f);
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
