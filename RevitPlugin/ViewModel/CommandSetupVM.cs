using CommunityToolkit.Mvvm.Input;
using RevitPlugin.Model;
using RevitPlugin.View;
using RevitPlugin.View.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RevitPlugin.ViewModel
{
	public class CommandSetupVM
	{
		public PluginUI ui;

		public List<RoomParameters> LayoutParameters { get; } = new List<RoomParameters>
		{
			new RoomParameters("Кухня", 2.8, 10, 0.5),
			new RoomParameters("Коридор", 1.2, 5, 0.5),
			new RoomParameters("Ванная", 1.7, 4, 0.5)
		};
		public RelayCommand NextPage { get => new RelayCommand(GoToNextPage); }
		public RelayCommand PreviousPage { get => new RelayCommand(GoToPreviousPage); }
		public RelayCommand Cancel { get => new RelayCommand(CancelingOperation); }

		public CommandSetupVM(PluginUI ui)
		{
			this.ui = ui;
			var frame = new LayoutParameters() { DataContext = this };
			frame.table.InitializingNewItem += (sender, args) =>
			{
				((RoomParameters)args.NewItem).Name = $"Комната {LayoutParameters.Count - 3}";
			};
			this.ui.mainFrame.Navigate(frame);
		}

		public void GoToNextPage()
		{
			var currentPage = ui.mainFrame.Content;
			Page nextPage;
			if (currentPage is LayoutParameters)
			{
				nextPage = new GeneratorParameters();
			}
			else if (currentPage is GeneratorParameters)
			{
				nextPage = new SelectLayout(); //new WaitFinish();
			}
			else throw new Exception("Unexpexted page in main frame");

			nextPage.DataContext = this;
			ui.mainFrame.Navigate(nextPage);
		}

		public void GoToPreviousPage()
		{
			var currentPage = ui.mainFrame.Content;
			Page nextPage;
			if (currentPage is SelectLayout || currentPage is WaitFinish)
			{
				nextPage = new GeneratorParameters();
			}
			else if (currentPage is GeneratorParameters)
			{
				nextPage = new LayoutParameters();
			}
			else throw new Exception("Unexpexted page in main frame");

			nextPage.DataContext = this;
			ui.mainFrame.Navigate(nextPage);
		}

		public void CancelingOperation()
		{
			ui.DialogResult = false;
			ui.Close();
		}

		public void ProgressBarValueChange(double value)
		{
			var waitPage = ui.mainFrame.Content as WaitFinish;
			var progressBar = waitPage.FindName("progressBar") as ProgressBar;
			progressBar.Value = value;
			if (progressBar.Value == progressBar.Maximum)
			{
				ui.mainFrame.Navigate(new SelectLayout() { DataContext = this });
			}
		}
	}
}
