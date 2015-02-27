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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace CodeReview
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel viewModel;
		public CodeReview cr;

		public MainWindow()
		{
			InitializeComponent();

			InitializeView();
		}

		private void InitializeView()
		{
			cr = new CodeReview();
			this.viewModel = new MainWindowViewModel(cr);
			this.DataContext = this.viewModel;
			this.IncidentDataGrid.ItemsSource = this.viewModel.FileObjects;
		}
	}

	public class Command : ICommand
	{
		public Command(Action<object> action) { this.action = action; }
		public bool CanExecute(object parameter) { return true; }
		public void Execute(object parameter) { action(parameter); }
		public event EventHandler CanExecuteChanged { add { } remove { } }
		private Action<object> action;
	}
}
