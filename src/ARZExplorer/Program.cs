//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Exceptions;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Services.Win32;

namespace ArzExplorer;

/// <summary>
/// Holds the main program.
/// </summary>
public static class Program
{
	// Set first thing in Main, before any form is created.
	internal static IServiceProvider ServiceProvider = null!;

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		// Setup regular Microsoft.Extensions.Logging abstraction manualy
		ILoggerFactory loggerFactory = new LoggerFactory();
		loggerFactory.AddLog4Net();

	restart:;
		// Configure DI
		var scol = new ServiceCollection()
		// Logs
		.AddSingleton(loggerFactory)// Register factory
		.AddSingleton(typeof(ILogger<>), typeof(Logger<>))
		// Abstractions
		.AddTransient<IFileIO, FileIO>()
		.AddTransient<IPathIO, PathIO>()
		.AddTransient<IDirectoryIO, DirectoryIO>()
		// Config
		.AddSingleton<UserSettings>(sp => UserSettings.Read())
		// Providers
		.AddTransient<IArcFileProvider, ArcFileProvider>()
		.AddTransient<IArzFileProvider, ArzFileProvider>()
		.AddTransient<IDBRecordCollectionProvider, DBRecordCollectionProvider>()
		// Services
		.AddSingleton<ITQDataService, TQDataService>()
		.AddTransient<IBitmapService, BitmapService>()
		.AddSingleton<IGamePathService, GamePathServiceWin>()
		// Init SoundServiceWin without IDatabase
		.AddSingleton<SoundServiceWin>(sp => new SoundServiceWin(sp.GetRequiredService<ILogger<SoundServiceWin>>(), null, sp.GetRequiredService<UserSettings>()))
		// Forms
		.AddSingleton<MainForm>()
		.AddTransient<ExtractProgress>();

		Program.ServiceProvider = scol.BuildServiceProvider();

		var gamePathResolver = Program.ServiceProvider.GetRequiredService<IGamePathService>();
		var userSettings = Program.ServiceProvider.GetRequiredService<UserSettings>();

		try
		{
			gamePathResolver.GamePathTQ = gamePathResolver.ResolveGamePath();
			gamePathResolver.GamePathTQIT = gamePathResolver.ResolveGamePath();
		}
		catch (ExGamePathNotFound ex)
		{
			using (var fbd = new FolderBrowserDialog() { Description = ex.Message, ShowNewFolderButton = false })
			{
				DialogResult result = fbd.ShowDialog();

				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					userSettings.ForceGamePath = fbd.SelectedPath;
					userSettings.Save();
					goto restart;
				}
				else goto exit;
			}
		}

		var mainform = Program.ServiceProvider.GetRequiredService<MainForm>();
		Application.Run(mainform);

	exit:;
	}


	/// <summary>
	/// Reads a value from the registry
	/// </summary>
	/// <param name="key">registry hive to be read</param>
	/// <param name="path">path of the value to be read</param>
	/// <returns>string value of the key and value that was read or empty string if there was a problem.</returns>
	internal static string ReadRegistryKey(Microsoft.Win32.RegistryKey key, string[] path)
	{
		// The last item in the path array is the actual value name.
		int valueKey = path.Length - 1;

		// All other values in the path are keys which will need to be navigated.
		int lastSubKey = path.Length - 2;

		for (int i = 0; i <= lastSubKey; ++i)
		{
			var subKey = key.OpenSubKey(path[i]);
			if (subKey is null)
			{
				return string.Empty;
			}
			key = subKey;
		}

		return key.GetValue(path[valueKey]) as string ?? string.Empty;
	}
}