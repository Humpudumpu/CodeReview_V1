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
		public ListCollectionView collectionView;

		public MainWindow()
		{
			InitializeComponent();
			InitializeView();
			this.MouseWheel +=MainWindow_MouseWheel;
		}


		//Bad code. Doesnt do with the MVVM model.
		void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Delta > 0 )
			{
				this.viewModel.IncreaseFontSize();
			}
			else if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Delta < 0)
			{
				this.viewModel.DecreaseFontSize();
			}
		}

		private void InitializeView()
		{
			cr = new CodeReview();
			this.viewModel = new MainWindowViewModel(cr);
			this.DataContext = this.viewModel;
			this.collectionView = new ListCollectionView(this.viewModel.FileObjects);
			this.IncidentDataGrid.ItemsSource = this.collectionView;
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			MenuItem mI = e.Source as MenuItem;
			if (collectionView.GroupDescriptions.Count > 0)
				collectionView.GroupDescriptions.Clear();
			collectionView.GroupDescriptions.Add(new PropertyGroupDescription(mI.Name));
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
