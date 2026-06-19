using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using Avalonia.Themes.Fluent;
using TQVaultAE.GUI.Next.Views;

namespace TQVaultAE.GUI.Next.Desktop
{
	internal sealed class Program
	{
		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
		public static void Main(string[] args)
		{
			var builder = BuildAvaloniaApp();
			var lifetime = CustomBuildAvaloniaApp(builder, args);
			//builder.StartWithClassicDesktopLifetime(args);
			lifetime.Start(args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
#if DEBUG
				.WithDeveloperTools()
#endif
				.WithInterFont()
				.LogToTrace();

		public static ClassicDesktopStyleApplicationLifetime CustomBuildAvaloniaApp(AppBuilder builder, string[] args)
		{
			var lifetime = new ClassicDesktopStyleApplicationLifetime
			{
				Args = args,
				ShutdownMode = ShutdownMode.OnLastWindowClose,
			};

			builder
				.AfterSetup(builder => builder.Instance?.Styles.Add(new FluentTheme()))
				.SetupWithLifetime(lifetime);

			lifetime.MainWindow = new MainWindow();
			return lifetime;
		}
	}
}
