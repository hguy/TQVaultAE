using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using TQVaultAE.Config;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>What an item tooltip shows beyond the classic display ("Tooltip Enhanced" settings).</summary>
[Flags]
internal enum ItemTooltipView
{
	/// <summary>Classic display.</summary>
	Classic = 0,
	/// <summary>Attributes split into prefix, base and suffix.</summary>
	Detailed = 1,
	/// <summary>Each value rolled by the in-game jitter (database -> in-game).</summary>
	InGameRolls = 2,
	/// <summary>The possible min/max of each rolled attribute, on its right.</summary>
	ValueRanges = 4,
	/// <summary>The skills granted by the item in a panel on the right.</summary>
	GrantedSkills = 8,
	/// <summary>How many of each piece of the item's set are loaded, on its right.</summary>
	SetPieceCounts = 16,
}

public partial class ItemTooltip : BaseTooltip
{
	private static Dictionary<Item, ItemTooltip> ItemTooltipOpened = new Dictionary<Item, ItemTooltip>();
	/// <summary>Rendered item tooltips and their text, up to 48 MB.</summary>
	private static readonly TooltipImageCache<TooltipKey, ToFriendlyNameResult> ToImage = new(48L << 20);

	/// <summary>What a cached tooltip image depends on. <paramref name="SetCounts"/> lists the set piece counts shown, empty without them.</summary>
	private readonly record struct TooltipKey(Item Item, float Scale, ItemTooltipView View, bool InGameValues, string SetCounts);

	internal ItemTooltipView View { get; set; }
	internal bool ShowInGameValues { get; set; }
	/// <summary>The loaded items, counted for <see cref="ItemTooltipView.SetPieceCounts"/>.</summary>
	internal IItemDatabaseService? ItemDatabase { get; set; }
	internal Item FocusedItem { get; set; }
	internal SackPanel? SackPanel { get; set; }
	internal ResultsDialog? ResultsDialog { get; set; }
	internal PictureBox? PictureBox { get; set; }


	/// <summary>Text of the item, set by <see cref="FillToolTip"/>.</summary>
	internal ToFriendlyNameResult? Data { get; private set; }

	/// <summary>Attribute rows with a right-aligned range, sized to the tooltip width once all rows exist.</summary>
	private readonly List<(Panel Row, Control Left, Control Right)> rangeRows = new();

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public ItemTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private ItemTooltip(MainForm instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService, Item focusedItem) : base(itemProvider, fontService, uiService, translationService)
	{
		InitializeComponent();

		this.FocusedItem = focusedItem;

		this.Owner = instance;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// A layered window, so that WS_EX_TRANSPARENT lets the mouse through (see CreateParams).
		this.TransparencyKey = SkillsTransparencyKey;

		var wa = Screen.FromControl(this).WorkingArea;

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, wa.Height);
	}

	public static void InvalidateCache(params Item?[] items)
	{
		var existing = items.OfType<Item>().ToArray();
		ToImage.RemoveWhere(k => existing.Contains(k.Item));
	}

	// to avoid Mainform lost focus with this.TopMost = false
	protected override bool ShowWithoutActivation => true;

	/// <summary>
	/// The mouse goes through the tooltip. A tooltip wider than the room beside the item covers the cursor:
	/// catching the mouse made the sack panel lose it and close the tooltip at once.
	/// </summary>
	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_TRANSPARENT = 0x20;
			var cp = base.CreateParams;
			cp.ExStyle |= WS_EX_TRANSPARENT;
			return cp;
		}
	}

	public static void HideTooltip()
	{
		lock (ToImage)
		{
			var lst = ItemTooltipOpened.Where(f => f.Value.Visible).ToList();
			foreach (var form in lst.Select(f => f.Value))
				form.Close();
			foreach (var key in lst.Select(f => f.Key))
				ItemTooltipOpened.Remove(key);
		}
	}

	#region Factory

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, SackPanel sackPanel
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();
				
			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				SackPanel = sackPanel,
				View = ViewOf(userSettings, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, ResultsDialog resultsDialog
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();

			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				ResultsDialog = resultsDialog,
				View = ViewOf(userSettings, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	public static ItemTooltip ShowTooltip(
		GuiServices services
		, Item focusedItem
		, PictureBox picBox
		, bool? enableDetailedTooltipView = null
	)
	{
		ItemTooltip _Current;
		lock (ToImage)
		{
			HideTooltip();

			var userSettings = services.Settings;

			_Current = new ItemTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
				, focusedItem)
			{
				PictureBox = picBox,
				View = ViewOf(userSettings, enableDetailedTooltipView),
				ShowInGameValues = userSettings.ShowInGameValues,
				ItemDatabase = services.ItemDatabaseService,
			};
			ItemTooltipOpened.Add(focusedItem, _Current);
			_Current.Show();
		}
		return _Current;
	}

	#endregion

	/// <summary>
	/// The display chosen in the settings: the classic one, or the detailed one with the extras checked in "Tooltip Enhanced".
	/// </summary>
	/// <param name="enableDetailedTooltipView">Forces the detailed display on or off, whatever the settings.</param>
	internal static ItemTooltipView ViewOf(UserSettings settings, bool? enableDetailedTooltipView = null)
	{
		if (!(enableDetailedTooltipView ?? settings.EnableDetailedTooltipView))
			return ItemTooltipView.Classic;

		var view = ItemTooltipView.Detailed;
		if (settings.TooltipShowInGameRolls)
			view |= ItemTooltipView.InGameRolls;
		if (settings.TooltipShowValueRanges)
			view |= ItemTooltipView.ValueRanges;
		if (settings.TooltipShowGrantedSkills)
			view |= ItemTooltipView.GrantedSkills;
		if (settings.TooltipShowSetPieceCounts)
			view |= ItemTooltipView.SetPieceCounts;
		return view;
	}

	/// <summary>
	/// Init the tooltip content.
	/// </summary>
	public void FillToolTip()
	{
		// How many of each set piece are loaded now: part of the cache key, since it changes with the loaded items.
		IReadOnlyDictionary<RecordId, int>? setCounts = null;
		if (this.View.HasFlag(ItemTooltipView.SetPieceCounts) && this.ItemDatabase is not null
			&& this.ItemProvider.GetSetItems(this.FocusedItem) is { setMembers.Count: > 0 } set)
			setCounts = this.ItemDatabase.CountItemsByRecord(set.setMembers.Keys.Select(RecordId.Create));

		var key = new TooltipKey(this.FocusedItem, UIService.Scale, this.View, this.ShowInGameValues
			, setCounts is null ? string.Empty : string.Join(",", setCounts.Values));
		bool detailed = this.View.HasFlag(ItemTooltipView.Detailed);

		// Redraw
		if (ToImage.TryGetCopy(key, out var cached, out var data))
		{
			this.Data = data;
			this.ShowCompositeImage(cached);
			return;
		}

		this.SuspendLayout();
		this.flowLayoutPanelFriendlyNames.SuspendLayout();
		this.rangeRows.Clear();

		// Without the skills panel, the granted skills are described inline as before.
		var scopes = FriendlyNamesExtraScopes.ItemFullDisplay;
		if (this.View.HasFlag(ItemTooltipView.GrantedSkills))
			scopes |= FriendlyNamesExtraScopes.GrantedSkills;
		this.Data = this.ItemProvider.GetFriendlyNames(FocusedItem, scopes);

		// Fullname
		AddRow(Data.FullName, FocusedItem.ExtractTextColorOrItemColor(Data.BaseItemInfoDescription), style: FontStyle.Bold);

		// Thrown
		if (Data.Item.IsThrownWeapon)
			AddRow(Data.BaseItemInfoStyle, style: FontStyle.Regular);

		// Artifact Level
		if (Data.Item.IsArtifact)
			AddRow(Data.ArtifactClass);

		// Relic Completion
		if (Data.Item.IsRelicOrCharm)
			AddRow(Data.RelicCompletionFormat, FocusedItem.ExtractTextColorOrItemColor(Data.BaseItemInfoDescription));

		// Recipe Label
		if (Data.Item.IsFormulae)
			AddRow(Data.ArtifactRecipe);

		// Flavor text
		if (Data.FlavorText.Length != 0)
		{
			foreach (var str in Data.FlavorText) AddRow(str);
			AddRow(TOOLTIPSPACER);
		}

		// Required Reagents
		if (Data.Item.IsFormulae)
		{
			AddRow(TOOLTIPSPACER);
			AddRow(Data.FormulaeFormat);
		}

		// Attributes
		if (detailed)
		{
			string AdjustColor(string? TQText, string label)
			{
				TQText = (TQText ?? string.Empty).InsertAfterColorPrefix($"{label} : ");
				return TQText.HasColorPrefix() ? TQText : $"{ItemStyle.Relic.TQColor().ColorTag()}{TQText}";
			}

			// Detailed display
			if (Data.PrefixAttributes.Length != 0)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(AdjustColor(this.Data.PrefixInfoDescription, Resources.ItemPropertiesLabelPrefixProperties), style: FontStyle.Bold);
				AddAttributeRows(Data.PrefixAttributes, Data.PrefixAttributeRanges);
			}
			if (Data.BaseAttributes.Length != 0)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(AdjustColor(this.Data.BaseItemInfoDescription, Resources.ItemPropertiesLabelBaseItemProperties), style: FontStyle.Bold);
				AddAttributeRows(Data.BaseAttributes, Data.BaseAttributeRanges);
			}
			if (Data.SuffixAttributes.Length != 0)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(AdjustColor(this.Data.SuffixInfoDescription, Resources.ItemPropertiesLabelSuffixProperties), style: FontStyle.Bold);
				AddAttributeRows(Data.SuffixAttributes, Data.SuffixAttributeRanges);
			}
		}
		else
		{
			// Classic display
			foreach (var str in Data.BaseAttributes) AddRow(str);
			foreach (var str in Data.PrefixAttributes) AddRow(str);
			foreach (var str in Data.SuffixAttributes) AddRow(str);
		}

		// In-game jitter comparison (database -> in-game), detailed view only
		if (this.View.HasFlag(ItemTooltipView.InGameRolls) && Data.InGameRolls.Length != 0)
		{
			AddRow(TOOLTIPSPACER);
			AddRow(Resources.ItemPropertiesLabelInGameRolls, ItemStyle.Relic.TQColor().Color(), style: FontStyle.Bold);
			foreach (var str in Data.InGameRolls) AddRow(str, ItemStyle.Broken.TQColor().Color());
		}

		// formula Artifact Details goes after formula attributes
		if (Data.FormulaeArtifactAttributes.Length != 0)
		{
			AddRow(TOOLTIPSPACER);
			AddRow(Data.FormulaeArtifactName, FocusedItem.ExtractTextColorOrItemColor(Data.BaseItemInfoDescription), style: FontStyle.Bold);
			AddRow(Data.FormulaeArtifactClass, ItemStyle.Broken.TQColor().Color());
			foreach (var str in Data.FormulaeArtifactAttributes) AddRow(str);
		}

		// Relic attributes after items attributes with delimiter.
		// Besides real relics and charms, a relic slot may hold any record: God mode in the Forge ignores the rules
		// (e.g. a weapon used as a relic), and what was socketed must still show.
		bool hasSocketedRecord = !Data.Item.IsRelicOrCharm && !Data.Item.IsArtifact
			&& (Data.Item.RelicInfo is not null || Data.Item.Relic2Info is not null);
		int relicRowsStart = this.flowLayoutPanelFriendlyNames.Controls.Count;
		if (Data.Item.HasRelicOrCharm || hasSocketedRecord)
		{
			if (Data.Relic1Attributes.Length != 0)
			{
				AddRow(TOOLTIPDELIM);
				AddRow(Data.RelicInfo1Description, ItemStyle.Relic.TQColor().Color(), style: FontStyle.Bold);// Name
				AddRow(Data.RelicInfo1CompletionResolved, ItemStyle.Relic.TQColor().Color());// Completion label
				foreach (var str in Data.Relic1Attributes) AddRow(str);
				AddRow(Data.RelicInfo1CompletionBonusResolved, ItemStyle.Relic.TQColor().Color());// Bonus Completion label
				foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
			}
			if (Data.Relic2Attributes.Length != 0)
			{
				AddRow(TOOLTIPDELIM);
				AddRow(Data.RelicInfo2Description, ItemStyle.Relic.TQColor().Color(), style: FontStyle.Bold);// Name
				AddRow(Data.RelicInfo2CompletionResolved, ItemStyle.Relic.TQColor().Color());// Completion label
				foreach (var str in Data.Relic2Attributes) AddRow(str);
				AddRow(Data.RelicInfo2CompletionBonusResolved, ItemStyle.Relic.TQColor().Color());// Bonus Completion label
				foreach (var str in Data.RelicBonus2Attributes) AddRow(str);
			}
		}
		else if (Data.Item.IsArtifact)
		{
			if (Data.RelicBonus1Attributes.Length != 0)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(Data.ArtifactBonus, ItemStyle.Relic.TQColor().Color());// Completion label
				foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
			}
		}
		else if (Data.Item.IsRelicOrCharm)
		{
			if (Data.RelicBonus1Attributes.Length != 0)
			{
				AddRow(TOOLTIPSPACER);
				AddRow(Data.RelicBonusTitle, ItemStyle.Relic.TQColor().Color());// Completion label
				foreach (var str in Data.RelicBonus1Attributes) AddRow(str);
			}
		}

		int relicRowsEnd = this.flowLayoutPanelFriendlyNames.Controls.Count;

		AddRow(TOOLTIPDELIM);

		// Add the item seed
		AddRow(Data.ItemSeed, ItemStyle.Broken.TQColor().Color());

		// Add the Atlantis clause
		if (Data.Item.IsAtlantis)
			AddRow(this.TranslationService.ItemAtlantis, TQColor.Green.Color());
		// Add the Eternal Embers clause
		else if (Data.Item.IsEmbers)
			AddRow(this.TranslationService.ItemEmbers, TQColor.Green.Color());
		// Add the Ragnarok clause
		else if (Data.Item.IsRagnarok)
			AddRow(this.TranslationService.ItemRagnarok, TQColor.Green.Color());
		// Add the Immortal Throne clause
		else if (Data.Item.IsImmortalThrone)
			AddRow(this.TranslationService.ItemIT, TQColor.Green.Color());

		// ItemSet
		if (Data.ItemSet is { setMembers.Count: > 0 })
		{
			AddRow(TOOLTIPDELIM);
			foreach (var str in Data.ItemSet.Translations)
			{
				// The set name comes first, then one line per piece with its count on the right.
				if (setCounts is not null && str.Key != Data.ItemSet.setName
					&& setCounts.TryGetValue(RecordId.Create(str.Key), out var count))
					this.flowLayoutPanelFriendlyNames.Controls.Add(this.MakeRangeRow(str.Value, count.ToString()));
				else
					AddRow(str.Value);
			}
		}

		if (Data.Requirements.Length != 0)
		{
			AddRow(TOOLTIPDELIM);
			// Requierments
			foreach (var str in Data.Requirements) AddRow(str, ItemStyle.Broken.TQColor().Color());
		}

		this.AlignRangeRows();
		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		// A tooltip taller than the screen gets cut: the relics then go in a column on the right.
		FlowLayoutPanel? relicColumn = null;
		if (relicRowsEnd > relicRowsStart
			&& this.flowLayoutPanelFriendlyNames.GetPreferredSize(Size.Empty).Height > this.MaxPanelHeight())
			relicColumn = this.MoveRowsToColumn(relicRowsStart, relicRowsEnd);

#if DEBUG
		if (this.AutomationRecorder is { } recorder)
		{
			recorder.Panel(this.flowLayoutPanelFriendlyNames);
			if (relicColumn is not null)
			{
				recorder.Column("relics");
				recorder.Panel(relicColumn);
			}
		}
#endif

		// The relic column and the granted skills go on the right, shown with the item panel as one cached image.
		if (relicColumn is not null || this.Data.GrantedSkills.Length != 0)
		{
			this.ShowComposite(key, relicColumn, this.Data.GrantedSkills);
			return;
		}

		// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw,
		// with the gray frame the form padding draws around it: the cached redraw has no padding.
		using var panel = Rasterize(this.flowLayoutPanelFriendlyNames);
		var raster = new Bitmap(panel.Width + 2, panel.Height + 2);
		using (var g = Graphics.FromImage(raster))
			DrawFramed(g, panel, 0, this.BackColor);
		ToImage.Add(key, raster, Data);
	}

	private void AddRow(string? friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
	{
		Control? row = MakeRow(this.UIService, this.FontService, friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	/// <summary>
	/// Adds attribute lines, each with its possible in-game values right-aligned when known (detailed view only).
	/// </summary>
	private void AddAttributeRows(string[] lines, string?[] ranges)
	{
		if (!this.View.HasFlag(ItemTooltipView.ValueRanges))
			ranges = [];

		for (int i = 0; i < lines.Length; i++)
		{
			var range = i < ranges.Length ? ranges[i] : null;
			if (string.IsNullOrEmpty(range))
				AddRow(lines[i]);
			else
				this.flowLayoutPanelFriendlyNames.Controls.Add(this.MakeRangeRow(lines[i], range));
		}
	}

	/// <summary>
	/// A row with the attribute on the left and its range on the right. Its width is set by <see cref="AlignRangeRows"/>:
	/// a flow layout does not count rows stretched with anchors when it measures its widest row.
	/// </summary>
	private Panel MakeRangeRow(string line, string range)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var left = MakeRow(this.UIService, this.FontService, line, BGColor: bg)
			?? MakeSingleColorLabel(this.UIService, this.FontService, line, null, 10F, FontStyle.Regular, bg);
		var right = MakeSingleColorLabel(this.UIService, this.FontService, range, ItemStyle.Broken.TQColor().Color(), 10F, FontStyle.Regular, bg);

		var row = new Panel()
		{
			AutoSize = false,
			BackColor = bg,
			Margin = new Padding(0),
			Padding = new Padding(0),
			Anchor = AnchorStyles.Left,
		};

		left.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		left.Location = Point.Empty;
		right.Anchor = AnchorStyles.Top | AnchorStyles.Left;

		row.Controls.Add(left);
		row.Controls.Add(right);
		this.rangeRows.Add((row, left, right));
		return row;
	}

	/// <summary>
	/// Gives every range row the width of the widest tooltip row (or its own natural width if larger),
	/// so ranges line up on the right edge without truncating the attribute text.
	/// </summary>
	private void AlignRangeRows()
	{
		if (this.rangeRows.Count == 0)
			return;

		var gap = (int)(16 * this.UIService.Scale);
		int width = 0;
		foreach (Control control in this.flowLayoutPanelFriendlyNames.Controls)
		{
			if (control is Panel panel && this.rangeRows.Exists(r => r.Row == panel))
				continue;
			if ((control.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
				continue; // stretched delimiters follow the others
			width = Math.Max(width, control.PreferredSize.Width + control.Margin.Horizontal);
		}

		foreach (var (_, left, right) in this.rangeRows)
			width = Math.Max(width, left.PreferredSize.Width + gap + right.PreferredSize.Width);

		foreach (var (row, left, right) in this.rangeRows)
		{
			var leftSize = left.PreferredSize;
			var rightSize = right.PreferredSize;
			row.Size = new Size(width, Math.Max(leftSize.Height, rightSize.Height));
			left.Size = leftSize;
			right.Size = rightSize;
			right.Location = new Point(width - rightSize.Width, 0);
		}
	}

	private void ItemTooltip_Load(object? sender, EventArgs e)
	{
		var usize = UIService.ItemUnitSize;
		var wa = Screen.FromControl(this).WorkingArea;

		this.FillToolTip();

		// Move it to view area from Mainform
		if (this.SackPanel != null)
		{
			var loc = this.SackPanel.PointToScreen(Point.Empty);
			var x = loc.X + ((this.FocusedItem.PositionX + this.FocusedItem.Size.Width) * usize);
			var y = loc.Y + (this.FocusedItem.PositionY * usize);

			// Adjust for Equipment panel weapon slot
			if (this.SackPanel.SackType == SackType.Equipment)
			{
				if (this.FocusedItem.IsInWeaponSlot)
				{
					var itemX = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).X;
					var itemY = SackCollection.GetWeaponLocationOffset(this.FocusedItem.PositionY).Y;
					x = loc.X + ((itemX + 2) * usize);
					y = loc.Y + (itemY * usize);
				}

				if (this.FocusedItem.IsAmulet)
					// Adjust for the whole size of the amulet box.
					x += (SackCollection.GetEquipmentLocationSize(1).Width - this.FocusedItem.Size.Width) * usize;
			}

			// Adjust position to avoid flickering if the tooltip appears on top of the cursor.
			var itemLeft = x - (this.FocusedItem.Size.Width * usize);
			x += Cursor.Size.Width;

			// The skills panel makes the tooltip wide: keep it on the screen of the panel,
			// on the left of the item when only that side has room, else as far right as it fits.
			var screen = Screen.FromPoint(loc).WorkingArea;
			if (x + this.Width > screen.Right)
			{
				var left = itemLeft - Cursor.Size.Width - this.Width;
				x = left >= screen.Left ? left : Math.Max(screen.Left, screen.Right - this.Width);
			}

			// Ajust position if tooltip size goes offscreen
			var bottom = y + this.Height;
			if (bottom > wa.Height)
			{
				// Maximize vertical view
				var offScreenHeight = bottom - wa.Height;
				if (y - offScreenHeight < 0)
					y = 0;// Do your best
				else
					y -= offScreenHeight;
			}

			this.Location = new Point(x, y);
			return;
		}

		// Move it to view area from Search form
		if (this.ResultsDialog != null)
		{
			var loc = this.ResultsDialog.PointToScreen(Point.Empty);
			var x = loc.X + this.ResultsDialog.Width;
			var y = loc.Y;

			this.Location = new Point(x, y);
		}

		// Move it to view area from PictureBox (Forge)
		if (this.PictureBox != null)
		{
			var loc = this.PictureBox.PointToScreen(Point.Empty);
			var x = loc.X + this.PictureBox.Width;
			var y = loc.Y;

			this.Location = new Point(x, y);
		}
	}
}