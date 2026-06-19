using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Linq;
using TQVaultAE.GUI.Next.ViewModels;
using TQVaultAE.GUI.Next.Views;

namespace TQVaultAE.GUI.Next
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new MainWindow
				{
					DataContext = new MainViewModel()
				};
			}
			else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
			{
				singleViewFactoryApplicationLifetime.MainViewFactory = () => new PageNavigationHost()
				{
					Page = new MainView { DataContext = new MainViewModel() }
				};
			}
			else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
			{
				singleViewPlatform.MainView = new PageNavigationHost()
				{
					Page = new MainView { DataContext = new MainViewModel() }
				};
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}