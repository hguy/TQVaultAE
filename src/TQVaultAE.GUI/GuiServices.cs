using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;

namespace TQVaultAE.GUI;

/// <summary>
/// Services shared by the forms, panels, bag buttons and tooltips of the GUI.
/// </summary>
/// <remarks>
/// Windows Forms controls are created by hand (<c>new SackPanel(...)</c>), so they cannot get their services by
/// constructor injection one by one. This bundle is injected once into each form and passed down to the controls
/// it creates, so every dependency of the GUI is listed here instead of being fetched from the container.
/// </remarks>
public sealed class GuiServices
{
	private readonly ILoggerFactory _loggerFactory;

	public GuiServices(
		ILoggerFactory loggerFactory
		, FormFactory forms
		, UserSettings settings
		, SessionContext session
		, IFontService fontService
		, IUIService uiService
		, ITranslationService translationService
		, IBitmapService bitmapService
		, ISoundService soundService
		, IDatabase database
		, IItemProvider itemProvider
		, IPlayerCollectionProvider playerCollectionProvider
		, ITQDataService tqData
		, IGamePathService gamePathService
		, IGameFileService gameFileService
		, IHighlightService highlightService
		, IItemDatabaseService itemDatabaseService
		, IItemExchangeService itemExchangeService
		, IIconService iconService
		, IPathIO pathIO
		, IFileIO fileIO
		, IDirectoryIO directoryIO
	)
	{
		this._loggerFactory = loggerFactory;
		this.Forms = forms;
		this.Settings = settings;
		this.Session = session;
		this.FontService = fontService;
		this.UIService = uiService;
		this.TranslationService = translationService;
		this.BitmapService = bitmapService;
		this.SoundService = soundService;
		this.Database = database;
		this.ItemProvider = itemProvider;
		this.PlayerCollectionProvider = playerCollectionProvider;
		this.TQData = tqData;
		this.GamePathService = gamePathService;
		this.GameFileService = gameFileService;
		this.HighlightService = highlightService;
		this.ItemDatabaseService = itemDatabaseService;
		this.ItemExchangeService = itemExchangeService;
		this.IconService = iconService;
		this.PathIO = pathIO;
		this.FileIO = fileIO;
		this.DirectoryIO = directoryIO;
	}

	/// <summary>Creates dialogs and gives access to the main form.</summary>
	public FormFactory Forms { get; }

	public UserSettings Settings { get; }
	public SessionContext Session { get; }
	public IFontService FontService { get; }
	public IUIService UIService { get; }
	public ITranslationService TranslationService { get; }
	public IBitmapService BitmapService { get; }
	public ISoundService SoundService { get; }
	public IDatabase Database { get; }
	public IItemProvider ItemProvider { get; }
	public IPlayerCollectionProvider PlayerCollectionProvider { get; }
	public ITQDataService TQData { get; }
	public IGamePathService GamePathService { get; }
	public IGameFileService GameFileService { get; }
	public IHighlightService HighlightService { get; }
	public IItemDatabaseService ItemDatabaseService { get; }
	public IItemExchangeService ItemExchangeService { get; }
	public IIconService IconService { get; }
	public IPathIO PathIO { get; }
	public IFileIO FileIO { get; }
	public IDirectoryIO DirectoryIO { get; }

	/// <summary>Logger for <typeparamref name="T"/>.</summary>
	public ILogger<T> Logger<T>() => this._loggerFactory.CreateLogger<T>();
}

/// <summary>
/// Creates the forms registered in the container: the only GUI code allowed to resolve from it.
/// </summary>
/// <remarks>
/// Dialogs are transient (a new instance per call); <see cref="MainForm"/> is the application singleton.
/// </remarks>
public sealed class FormFactory
{
	private readonly IServiceProvider _provider;

	public FormFactory(IServiceProvider provider)
		=> this._provider = provider;

	/// <summary>The main window, owner of the item and bag tooltips.</summary>
	public MainForm MainForm => this._provider.GetRequiredService<MainForm>();

	/// <summary>Creates (or, for singletons, returns) the form <typeparamref name="TForm"/>.</summary>
	public TForm Create<TForm>() where TForm : Form
		=> this._provider.GetRequiredService<TForm>();
}
