using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that create the app's forms and dialogs directly, to check them without finding the click that opens them.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "list_forms", ReadOnly = true)]
	[Description("Lists the form and dialog types registered in the container, which open_form can create.")]
	public static string ListForms()
		=> Serialize(FormTypes().Select(t => new { name = t.Name, type = t.FullName }));

	[McpServerTool(Name = "open_form", Destructive = true)]
	[Description("Creates a form or dialog from the container, as the app does (FormFactory), so its InitializeComponent and resources run. With show=false (the default) it reports the created form and disposes it; with show=true it opens it modeless over the main window, to capture or close afterwards with capture and close_dialog. Singletons (MainForm, BagButtonSettings) are only reported. Dialogs that need data first (ResultsDialog, ItemProperties...) may fail when shown: see recent_log.")]
	public static string OpenForm(
		[Description("Type name, as listed by list_forms, e.g. AboutBox or SettingsDialog.")] string name,
		[Description("true to show the form modeless instead of disposing it.")] bool show = false)
		=> AutomationHost.OnUi(main =>
		{
			var type = FormTypes().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
				?? throw new McpException($"Unknown form '{name}'. Forms: {string.Join(", ", FormTypes().Select(t => t.Name))}");

			AutomationHost.MarkDriving();
			var form = (Form)Program.ServiceProvider.GetRequiredService(type);

			// A transient registration returns a new instance on each call.
			var other = (Form)Program.ServiceProvider.GetRequiredService(type);
			var singleton = ReferenceEquals(form, other);
			if (!singleton)
				other.Dispose();

			var report = new
			{
				type = type.FullName,
				singleton,
				title = form.Text,
				size = new { width = form.Width, height = form.Height },
				controls = CountControls(form),
				backgroundImage = form.BackgroundImage is not null,
				icon = form.Icon is not null,
				shown = show && !singleton,
			};

			if (singleton)
				return Serialize(report);

			if (show)
			{
				// Shown after this call returns, so a Load handler opening a message box cannot block the endpoint.
				main.BeginInvoke(() =>
				{
					try
					{
						form.Show(main);
					}
					catch (Exception ex)
					{
						AutomationHost.Log?.LogError(ex, "open_form: {Form} failed to show", type.Name);
						form.Dispose();
					}
				});
			}
			else
			{
				form.Dispose();
			}

			return Serialize(report);
		});

	private static IEnumerable<Type> FormTypes()
	{
		var check = Program.ServiceProvider.GetRequiredService<IServiceProviderIsService>();
		return typeof(Program).Assembly.GetTypes()
			.Where(t => typeof(Form).IsAssignableFrom(t) && !t.IsAbstract && check.IsService(t))
			.OrderBy(t => t.Name);
	}

	private static int CountControls(Control control)
		=> control.Controls.Cast<Control>().Sum(c => 1 + CountControls(c));
}
