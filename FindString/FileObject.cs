using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindString
{
	public class FileObject
	{
		public string DevBranch { get; set; }
		public string Filename { get; set; }
		public string Comments { get; set; }
		public int CheckOutChangeSet { get; set; }
		public int CheckInChangeSet { get; set; }
		public string Author { get; set; }

		public FileObject() 
		{ }

		//Contructor used to test the observable collection
		public FileObject(string filename, string devBranch, string comments, int checkoutNo, int checkinNo, string author)
		{
			this.DevBranch = devBranch;
			this.Filename = filename;
			this.Comments = comments;
			this.CheckOutChangeSet = checkoutNo;
			this.CheckInChangeSet = checkinNo;
			this.Author = author;
		}
	}
}
