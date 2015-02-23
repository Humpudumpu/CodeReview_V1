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

		public List<FileObject> fileList;
		public CodeReview()
		{
			fileList = new List<FileObject>();
		}

		public void GetIncident(int incident)
		{
			//Test code
			//fileList.Add(new FileObject("filename1", "devBranch1", "comments1", 10, 16, "Author1"));
			//fileList.Add(new FileObject("filename2", "devBranch2", "comments2", 20, 26, "Author2"));
			//fileList.Add(new FileObject("filename3", "devBranch3", "comments3", 30, 36, "Author3"));
			//fileList.Add(new FileObject("filename4", "devBranch4", "comments4", 40, 46, "Author4"));
			//fileList.Add(new FileObject("filename5", "devBranch5", "comments5", 50, 56, "Author5"));
			//fileList.Add(new FileObject("filename6", "devBranch6", "comments6", 60, 66, "Author6"));
			//fileList.Add(new FileObject("filename6", "devBranch7", "comments7", 70, 76, "Author7"));

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
