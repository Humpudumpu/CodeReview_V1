using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodeReview
{
	/// <summary>
	/// Interaction logic for ReviewNotes.xaml
	/// </summary>
	public partial class ReviewNotes : Window
	{
		private ReviewNotesViewModel viewModel;

		public ReviewNotes(ReviewNotesViewModel viewModel)
		{
			InitializeComponent();
			this.viewModel = viewModel;
		}
	}
}
