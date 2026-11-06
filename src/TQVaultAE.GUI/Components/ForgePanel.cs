using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Forge: merges the prefix, suffix and relics of material items into a base item, under the rules of a
/// <see cref="ForgeMode"/> (<see cref="ForgeRules"/>).
/// </summary>
/// <remarks>
/// While an item is dragged over the panel it is shown as the mouse cursor (<see cref="ItemDragCursor"/>), so moving
/// the mouse repaints nothing; clicking a slot drops it there.
/// </remarks>
public partial class ForgePanel : UserControl, IScalingControl
{
	#region Internal Types

	private sealed class ForgeComboItem
	{
		public ForgePart Value;
		public string Text;
		public override string ToString() => Text;
	}

	/// <summary>Controls of one slot.</summary>
	private sealed record SlotView(ForgeSlot Slot, PictureBox Picture, ComboBox? Combo);

	#endregion

	private readonly ISoundService SoundService;
	private readonly IUIService UIService;
	private readonly IFontService FontService;
	private readonly ITranslationService TranslationService;
	private readonly IItemProvider ItemProvider;
	private readonly IItemDatabaseService ItemDatabaseService;

	internal Action CancelAction;
	internal Action ForgeAction;

	/// <summary>Items placed in the slots, and the sacks they come from.</summary>
	private readonly ForgeSelection Selection = new();
	private readonly Dictionary<ForgeSlot, SackCollection?> Sacks = new();
	private readonly SlotView[] Slots;
	private readonly ForgeRules Rules;

	/// <summary>Cards describing what each mode allows, on the mode radio buttons.</summary>
	private RichToolTip? modeToolTip;

	private ScalingRadioButton lastMode;

	private static RecordId SoundStricMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE01.WAV";
	private static RecordId SoundRelaxMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE02.WAV";
	private static RecordId SoundGameMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE03.WAV";
	private static RecordId SoundGodMode = @"Sounds\AMBIENCE\RANDOMEVENT\TYPHONLAUGHDISTANCE.WAV";

	/// <summary>
	/// Gets or sets the dragInfo instance of any items being dragged.
	/// </summary>
	protected ItemDragInfo DragInfo;
	private readonly GuiServices Services;

	/// <summary>Cursor showing the dragged item while over the panel, and the item it was made for.</summary>
	private ItemDragCursor? dragCursor;
	private Item? dragCursorItem;

	/// <summary>Backgrounds drawn once at the control size, instead of being stretched on every paint.</summary>
	private Bitmap? panelBackground;
	private Bitmap? tableBackground;

	private bool IsGodMode => scalingRadioButtonGod.Checked;
	private bool IsRelaxMode => scalingRadioButtonRelax.Checked;
	private bool IsStrictMode => scalingRadioButtonStrict.Checked;
	private bool IsGameMode => scalingRadioButtonGame.Checked;
	private bool IsDraging => DragInfo != null && DragInfo.IsActive;

	private ForgeMode Mode
		=> IsGodMode ? ForgeMode.God
		: IsRelaxMode ? ForgeMode.Relax
		: IsStrictMode ? ForgeMode.Strict
		: ForgeMode.Game;

	private readonly string OfTheTinkererTranslation;

	private readonly Color PictureBoxBaseColor = Color.FromArgb(32 * 7, 46, 41, 31);

	Item _PreviewItem = null;

	internal bool HardcoreMode;

	/// <summary>Base item with the current selection applied, for its tooltip.</summary>
	private Item PreviewItem
	{
		get
		{
			if (Selection.Base is null) return null;
			if (_PreviewItem is null)
			{
				_PreviewItem = Selection.Base.Clone();
				if (Selection.Prefix is not null || Selection.Suffix is not null || Selection.Relic1 is not null || Selection.Relic2 is not null)
				{
					ForgeMerge.Apply(_PreviewItem, Selection, IsGodMode, GodPart);
					this.ItemProvider.GetDBData(_PreviewItem);
				}
			}
			return _PreviewItem;
		}
	}

	public ForgePanel()
	{
		InitializeComponent();
	}

	public ForgePanel(ItemDragInfo dragInfo, GuiServices services)
	{
		InitializeComponent();

		DoubleBuffered = true;

		DragInfo = dragInfo;
		Services = services;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

		SoundService = services.SoundService;
		UIService = services.UIService;
		FontService = services.FontService;
		TranslationService = services.TranslationService;
		ItemProvider = services.ItemProvider;
		ItemDatabaseService = services.ItemDatabaseService;

		Rules = new ForgeRules(LevelRequirement);
		Slots = [
			new(ForgeSlot.Base, pictureBoxBaseItem, null),
			new(ForgeSlot.Prefix, pictureBoxPrefix, comboBoxPrefix),
			new(ForgeSlot.Suffix, pictureBoxSuffix, comboBoxSuffix),
			new(ForgeSlot.Relic1, pictureBoxRelic1, comboBoxRelic1),
			new(ForgeSlot.Relic2, pictureBoxRelic2, comboBoxRelic2),
		];

		#region Font, Scaling, Translate, BackGround

		this.RenderBackgrounds();
		this.SizeChanged += (_, _) => this.RenderBackgrounds();
		tableLayoutPanelForge.SizeChanged += (_, _) => this.RenderBackgrounds();

		// Reduce slightly the designed component to fit better the UI.
		foreach (var slot in Slots)
			slot.Picture.Size = new Size(Convert.ToInt32(slot.Picture.Size.Width * 0.9), Convert.ToInt32(slot.Picture.Size.Height * 0.9));

		scalingCheckBoxHardcore.Font =
		scalingRadioButtonGame.Font =
		scalingRadioButtonGod.Font =
		scalingRadioButtonRelax.Font =
		scalingRadioButtonStrict.Font =
			FontService.GetFont(10F, FontStyle.Regular, UIService.Scale);

		scalingLabelBaseItem.Font =
		scalingLabelPrefix.Font =
		scalingLabelRelic1.Font =
		scalingLabelRelic2.Font =
		scalingLabelSuffix.Font =
			FontService.GetFont(11F, FontStyle.Bold, UIService.Scale);

		scalingLabelBaseItem.ForeColor =
		scalingLabelPrefix.ForeColor =
		scalingLabelRelic1.ForeColor =
		scalingLabelRelic2.ForeColor =
		scalingLabelSuffix.ForeColor = TQColor.Yellow.Color();

		// Scalling
		var size = TextRenderer.MeasureText(scalingLabelBaseItem.Text, scalingLabelBaseItem.Font);
		scalingLabelBaseItem.Height =
		scalingLabelPrefix.Height =
		scalingLabelRelic1.Height =
		scalingLabelRelic2.Height =
		scalingLabelSuffix.Height = size.Height;

		ForgeButton.Font = FontService.GetFontLight(12F);
		ResetButton.Font = FontService.GetFontLight(12F);
		CancelButton.Font = FontService.GetFontLight(12F);

		// ComboBox scaling
		comboBoxPrefix.Font =
		comboBoxSuffix.Font =
		comboBoxRelic1.Font =
		comboBoxRelic2.Font =
			FontService.GetFont(9.75F, FontStyle.Regular, UIService.Scale);

		scalingRadioButtonGod.Text = Resources.ForgeGodMode;
		scalingRadioButtonGod.ForeColor = TQColor.Indigo.Color();

		OfTheTinkererTranslation = TranslationService.TranslateXTag("x3tagSuffix01");

		scalingRadioButtonRelax.Text = Resources.ForgeRelaxMode;
		scalingRadioButtonRelax.ForeColor = TQColor.Aqua.Color();

		scalingRadioButtonStrict.Text = Resources.ForgeStrictMode;
		scalingRadioButtonStrict.ForeColor = TQColor.Yellow.Color();

		scalingRadioButtonGame.Text = Resources.ForgeGameMode;
		scalingRadioButtonGame.ForeColor = TQColor.Green.Color();

		this.SetModeToolTips();

		scalingCheckBoxHardcore.Text = Resources.GlobalHardcore;
		toolTip.SetToolTip(scalingCheckBoxHardcore, Resources.ForgeHardcoreTT);
		scalingCheckBoxHardcore.ForeColor = TQColor.Red.Color();

		#endregion

		HideGodModeRows();

		// Everything in the Forge takes part in dragging
		this.WireDragEvents(this);
		this.ProcessAllControls(this.WireDragEvents);

		ResetSelection();
	}

	private void WireDragEvents(Control c)
	{
		c.MouseEnter += Forge_MouseEnter;
		c.MouseMove += Forge_MouseMove;
		c.MouseLeave += Forge_MouseLeave;
		c.Click += Forge_Click;
	}

	/// <summary>
	/// Describes on each mode radio button what the mode allows: one line per check of <see cref="ForgeRules"/>.
	/// </summary>
	private void SetModeToolTips()
	{
		this.modeToolTip = new RichToolTip(
			FontService.GetFont(11F, FontStyle.Bold, UIService.Scale),
			FontService.GetFont(9.5F, FontStyle.Regular, UIService.Scale),
			FontService.GetFont(10F, FontStyle.Bold, UIService.Scale),
			UIService.Scale);

		static RichToolTip.Line Allow(string text) => new(text, true);
		static RichToolTip.Line Limit(string text) => new(text, false);
		RichToolTip.Section Base(params RichToolTip.Line[] lines) => new(Resources.ForgeCapSectionBase, lines);
		RichToolTip.Section Affixes(params RichToolTip.Line[] lines) => new(Resources.ForgeCapSectionAffixes, lines);
		RichToolTip.Section Relics(params RichToolTip.Line[] lines) => new(Resources.ForgeCapSectionRelics, lines);
		var tinkerer = string.Format(Resources.ForgeCapSecondRelicTinkerer, OfTheTinkererTranslation);

		this.modeToolTip.Set(scalingRadioButtonGame, new RichToolTip.Card(
			Resources.ForgeGameMode, scalingRadioButtonGame.ForeColor, Resources.ForgeGameModeTT,
			[
				Base(Limit(Resources.ForgeCapBaseGear), Limit(Resources.ForgeCapNoEpicLegendary)),
				Affixes(
					Limit(Resources.ForgeCapSameBaseItem),
					Limit(Resources.ForgeCapLevelRequirement),
					Limit(Resources.ForgeCapSameGearType),
					Limit(Resources.ForgeCapNotRarerThanBase),
					Limit(Resources.ForgeCapNoEpicLegendary),
					Limit(Resources.ForgeCapNeedsAffix)),
				Relics(Limit(Resources.ForgeCapRelicOnly), Limit(Resources.ForgeCapRelicGear), Limit(tinkerer)),
			]));

		this.modeToolTip.Set(scalingRadioButtonStrict, new RichToolTip.Card(
			Resources.ForgeStrictMode, scalingRadioButtonStrict.ForeColor, Resources.ForgeStrictModeTT,
			[
				Base(Limit(Resources.ForgeCapBaseGear), Limit(Resources.ForgeCapNoEpicLegendary)),
				Affixes(
					Limit(Resources.ForgeCapSameGearType),
					Limit(Resources.ForgeCapNotRarerThanBase),
					Limit(Resources.ForgeCapNoEpicLegendary),
					Limit(Resources.ForgeCapNeedsAffix)),
				Relics(Limit(Resources.ForgeCapRelicOnly), Limit(Resources.ForgeCapRelicGear), Limit(tinkerer)),
			],
			Resources.ForgeCapResetOnStricter));

		this.modeToolTip.Set(scalingRadioButtonRelax, new RichToolTip.Card(
			Resources.ForgeRelaxMode, scalingRadioButtonRelax.ForeColor, string.Format(Resources.ForgeRelaxModeTT, OfTheTinkererTranslation),
			[
				Base(Limit(Resources.ForgeCapBaseGear), Allow(Resources.ForgeCapEpicLegendary)),
				Affixes(Allow(Resources.ForgeCapAnyGearType), Allow(Resources.ForgeCapEpicLegendary), Limit(Resources.ForgeCapNeedsAffix)),
				Relics(Allow(Resources.ForgeCapRelicOrSocketed), Limit(Resources.ForgeCapRelicGear), Allow(Resources.ForgeCapSecondRelicFree)),
			],
			Resources.ForgeCapResetOnStricter));

		this.modeToolTip.Set(scalingRadioButtonGod, new RichToolTip.Card(
			Resources.ForgeGodMode, scalingRadioButtonGod.ForeColor, Resources.ForgeGodModeTT,
			[
				Base(Allow(Resources.ForgeCapAnyItem)),
				Affixes(Allow(Resources.ForgeCapAnyPart)),
				Relics(Allow(Resources.ForgeCapAnyRelicSlot), Allow(Resources.ForgeCapSecondRelicFree)),
			],
			Resources.ForgeCapResetOnStricter));
	}

	private int LevelRequirement(Item item)
		=> this.ItemProvider.GetRequirementVariables(item).TryGetValue(Variable.KEY_LEVELREQ, out var level) ? level.GetInt32(0) : 0;

	#region Backgrounds

	private void RenderBackgrounds()
	{
		ReplaceBackground(this, ref this.panelBackground, () => Resources.caravan_bg);
		ReplaceBackground(this.tableLayoutPanelForge, ref this.tableBackground, () => Resources.StashPanel);
	}

	/// <summary>Sets <paramref name="control"/>'s background to the image drawn once at its size.</summary>
	/// <param name="load">Loads the source image (a new copy each call); only called when the size changed.</param>
	private static void ReplaceBackground(Control control, ref Bitmap? cache, Func<Bitmap> load)
	{
		var size = control.ClientSize;
		if (size.Width <= 0 || size.Height <= 0 || (cache is not null && cache.Size == size))
			return;

		using (var source = load())
		{
			var rendered = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			using (var g = Graphics.FromImage(rendered))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.DrawImage(source, new Rectangle(Point.Empty, size));
			}

			control.BackgroundImageLayout = ImageLayout.None;
			control.BackgroundImage = rendered;
			cache?.Dispose();
			cache = rendered;
		}
	}

	#endregion

	#region Dragging

	private void Forge_MouseEnter(object sender, EventArgs e)
	{
		// Enforce first autosize display
		if (MaximumSize == Size.Empty)
			MaximumSize = new Size(Size.Width, Size.Height + 50);

		this.UpdateDragCursor();
	}

	private void Forge_MouseMove(object sender, MouseEventArgs e) => this.UpdateDragCursor();

	private void Forge_MouseLeave(object sender, EventArgs e)
	{
		// Leaving a child for another one stays inside the panel
		if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
			this.EndDragCursor();
	}

	private void Forge_Click(object sender, EventArgs e)
	{
		if (!IsDraging)
			return;

		var slot = Slots.FirstOrDefault(s => s.Picture.Visible
			&& s.Picture.RectangleToScreen(s.Picture.ClientRectangle).Contains(Cursor.Position));
		if (slot is not null)
			this.DropDraggedItem(slot.Slot);
	}

	/// <summary>Shows the dragged item as the cursor, or the normal cursor when nothing is dragged.</summary>
	private void UpdateDragCursor()
	{
		if (!IsDraging)
		{
			this.EndDragCursor();
			return;
		}

		if (this.dragCursor is not null && ReferenceEquals(this.dragCursorItem, DragInfo.Item))
			return;

		this.EndDragCursor();
		var bitmap = UIService.LoadBitmap(DragInfo.Item.TexImageResourceId);
		if (bitmap is null)
			return;

		this.dragCursor = ItemDragCursor.Create(bitmap, UIService.Scale);
		this.dragCursorItem = DragInfo.Item;
		if (this.dragCursor is not null)
			this.Cursor = this.dragCursor.Cursor;
	}

	private void EndDragCursor()
	{
		if (this.dragCursor is null)
			return;
		this.Cursor = Cursors.Default;
		this.dragCursor.Dispose();
		this.dragCursor = null;
		this.dragCursorItem = null;
	}

	/// <summary>Puts the dragged item in <paramref name="slot"/> when the rules of the current mode allow it.</summary>
	private void DropDraggedItem(ForgeSlot slot)
	{
		if (DragInfo.SrcSack is null)
		{
			Notify(ForgeMessage.ItemMustBeStoredFirst);
			return;
		}

		var item = DragInfo.Item;
		if (Selection.Contains(item))
			return;

		var decision = Rules.CheckDrop(Mode, slot, item, Selection);

		foreach (var cleared in decision.Cleared)
			ClearSlot(cleared);
		foreach (var warning in decision.Warnings)
			Notify(warning);

		if (!decision.Accepted)
		{
			Notify(decision.Rejection!.Value);
			return;
		}

		PlaceInSlot(slot, item, DragInfo.SrcSack);
		DragInfo.Cancel();
		EndDragCursor();
		SoundService.PlayRandomItemDrop();
		ResetPreviewItem();
	}

	#endregion

	#region Slots

	private SlotView SlotOf(ForgeSlot slot) => Slots.First(s => s.Slot == slot);

	private void PlaceInSlot(ForgeSlot slot, Item item, SackCollection sack)
	{
		Selection[slot] = item;
		Sacks[slot] = sack;

		var view = SlotOf(slot);
		view.Picture.Image = UIService.LoadBitmap(item.TexImageResourceId);
		view.Picture.BackColor = Color.FromArgb(127, ControlPaint.Dark(item.ItemStyle.Color()));
		if (view.Combo is not null)
			FillPartComboBox(view.Combo, item);
	}

	private void ClearSlot(ForgeSlot slot)
	{
		Selection[slot] = null;
		Sacks[slot] = null;

		var view = SlotOf(slot);
		view.Picture.Image = null;
		view.Picture.BackColor = PictureBoxBaseColor;
		view.Combo?.Items.Clear();
	}

	private void ResetSelection()
	{
		foreach (var slot in Slots)
			ClearSlot(slot.Slot);

		ResetPreviewItem();
	}

	private void ResetPreviewItem()
	{
		if (_PreviewItem is not null)
			ItemTooltip.InvalidateCache(_PreviewItem);// Invalide old preview item
		_PreviewItem = null;
	}

	/// <summary>God mode: the part of the material item chosen in its combo box.</summary>
	private ForgePart? GodPart(ForgeSlot slot)
		=> (SlotOf(slot).Combo?.SelectedItem as ForgeComboItem)?.Value;

	/// <summary>Parts of <paramref name="item"/> usable in God mode.</summary>
	private static void FillPartComboBox(ComboBox comboBox, Item item)
	{
		comboBox.Items.Clear();
		comboBox.Items.Add(new ForgeComboItem { Value = ForgePart.Base, Text = Resources.ForgeUseBase });

		if (item.HasPrefix)
			comboBox.Items.Add(new ForgeComboItem { Value = ForgePart.Prefix, Text = Resources.ForgeUsePrefix });

		if (item.HasSuffix)
			comboBox.Items.Add(new ForgeComboItem { Value = ForgePart.Suffix, Text = Resources.ForgeUseSuffix });

		if (item.HasRelicOrCharmSlot1)
			comboBox.Items.Add(new ForgeComboItem { Value = ForgePart.Relic1, Text = Resources.ForgeUseRelic1 });

		if (item.HasRelicOrCharmSlot2)
			comboBox.Items.Add(new ForgeComboItem { Value = ForgePart.Relic2, Text = Resources.ForgeUseRelic2 });

		comboBox.SelectedIndex = 0;// default on Base
	}

	private void HideGodModeRows()
	{
		foreach (var slot in Slots)
			if (slot.Combo is not null)
				slot.Combo.Visible = false;
	}

	private void ShowGodModeRows()
	{
		foreach (var slot in Slots)
			if (slot.Combo is not null)
			{
				slot.Combo.SelectedIndex = -1;
				slot.Combo.Visible = true;
			}
	}

	#endregion

	#region Forge

	private void ForgeItem()
	{
		if (Rules.CheckForge(Selection) is { } problem)
		{
			Notify(problem);
			return;
		}

		// Merge item properties into Base item
		var itm = Selection.Base!;
		ForgeMerge.Apply(itm, Selection, IsGodMode, GodPart);
		ItemProvider.GetDBData(itm);

		Sacks[ForgeSlot.Base]!.IsModified = itm.IsModified = true;

		HardcoreDeleteMaterialItems();

		InvalidateItemCacheAll(itm);
		// The search keeps the item's display data: replace its entry so it shows the forged item.
		ItemDatabaseService.RemoveItemFromDatabase(itm);
		ItemDatabaseService.TryAddItemToDatabase(itm);

		ResetSelection();

		// Sound
		if (IsGodMode)
			SoundService.PlayLevelUp();
		else
			SoundService.PlayRandomMetalHit();

		// External Code
		if (ForgeAction is not null)
			ForgeAction();
	}

	/// <summary>Hardcore mode: the material items are consumed.</summary>
	private void HardcoreDeleteMaterialItems()
	{
		if (this.HardcoreMode)
		{
			foreach (var slot in new[] { ForgeSlot.Prefix, ForgeSlot.Suffix, ForgeSlot.Relic1, ForgeSlot.Relic2 })
			{
				if (Selection[slot] is not { } item || Sacks.GetValueOrDefault(slot) is not { } sack)
					continue;

				sack.RemoveItem(item);
				InvalidateItemCacheAll(item);
				ItemDatabaseService.RemoveItemFromDatabase(item);
			}
		}

		// Repaint the sacks the items left (and the forged item) at the next paint, not synchronously.
		this.FindForm()?.Invalidate(true);
	}

	private void InvalidateItemCacheAll(params Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		BagButtonTooltip.InvalidateCache(items);
		ItemProvider.InvalidateFriendlyNamesCache(items);
	}

	#endregion

	#region Messages

	private void Notify(ForgeMessage message)
	{
#if DEBUG
		this.AutomationRecordMessage(message);
#endif
		this.UIService.NotifyUser(TextOf(message), TQColor.Red);
		SoundService.PlayRandomCancel();
	}

	private string TextOf(ForgeMessage message) => message switch
	{
		ForgeMessage.ItemMustBeStoredFirst => Resources.ForgeItemMustBeStoredFirst,
		ForgeMessage.MustSelectBaseItemFirst => Resources.ForgeMustSelectBaseItemFirst,
		ForgeMessage.MustSelectArmorOrWeaponOrJewellery => Resources.ForgeMustSelectArmorOrWeaponOrJewellery,
		ForgeMessage.MustHavePrefix => Resources.ForgeMustHavePrefixOutsideOfGodMode,
		ForgeMessage.MustHaveSuffix => Resources.ForgeMustHaveSuffixOutsideOfGodMode,
		ForgeMessage.MustSelectRelicOrItemWithRelic => Resources.ForgeMustSelectRelicOrItemWithRelic,
		ForgeMessage.NoEpicLegendary => Resources.ForgeNoEpicLegendaryAllowed,
		ForgeMessage.PrefixSuffixOfAnotherBaseItem => Resources.ForgeCantUsePrefixSuffixOfAnotherBaseItem,
		ForgeMessage.PrefixSuffixHigherLevelRequirement => Resources.ForgeCantUsePrefixSuffixHavingGreaterReqLevelThanBaseItem,
		ForgeMessage.PrefixSuffixOfAnotherGearType => Resources.ForgeCantUsePrefixSuffixOfAnotherGearType,
		ForgeMessage.PrefixSuffixBetterThanBase => Resources.ForgeCantUsePrefixSuffixBetterThanBase,
		ForgeMessage.MustBeRelic => Resources.ForgeMustBeRelicInStrictMode,
		ForgeMessage.MustHaveRelic => Resources.ForgeMustHaveRelicInRelaxMode,
		ForgeMessage.RelicNotAllowed => Resources.ForgeRelicNotAllowed,
		ForgeMessage.MustHaveTinkeredSuffix => string.Format(Resources.ForgeMustHaveTinkeredSuffix, OfTheTinkererTranslation),
		ForgeMessage.MustSelectBaseItem => Resources.ForgeMustSelectBaseItem,
		ForgeMessage.MustSelectItemToMerge => Resources.ForgeMustSelectItemToMerge,
		_ => message.ToString(),
	};

	#endregion

	#region Event handlers

	private void CancelButton_Click(object sender, EventArgs e)
	{
		ResetSelection();

		if (CancelAction is not null)
			CancelAction();
	}

	private void ForgeButton_Click(object sender, EventArgs e) => ForgeItem();

	private void scalingButtonReset_Click(object sender, EventArgs e) => ResetSelection();

	private void ForgeMode_Clicked(object sender, EventArgs e)
	{
		if (IsGameMode)
		{
			SoundService.PlaySound(SoundGameMode);

			// Reset on Mode downgrade
			if (lastMode == scalingRadioButtonGod || lastMode == scalingRadioButtonRelax || lastMode == scalingRadioButtonStrict)
				ResetSelection();

			HideGodModeRows();
			lastMode = scalingRadioButtonGame;
			return;
		}

		if (IsStrictMode)
		{
			SoundService.PlaySound(SoundStricMode);

			// Reset on Mode downgrade
			if (lastMode == scalingRadioButtonGod || lastMode == scalingRadioButtonRelax)
				ResetSelection();

			HideGodModeRows();
			lastMode = scalingRadioButtonStrict;
			return;
		}

		if (IsRelaxMode)
		{
			SoundService.PlaySound(SoundRelaxMode);

			// Reset on Mode downgrade
			if (lastMode == scalingRadioButtonGod)
				ResetSelection();

			HideGodModeRows();
			lastMode = scalingRadioButtonRelax;
			return;
		}

		if (IsGodMode)
		{
			SoundService.PlaySound(SoundGodMode);

			ShowGodModeRows();
			lastMode = scalingRadioButtonGod;
		}
	}

	private void pictureBox_MouseEnter(object sender, EventArgs e)
	{
		// No tooltip over the slot the dragged item is about to be dropped on
		if (IsDraging)
			return;

		var slot = Slots.First(s => s.Picture == sender).Slot;
		var itm = slot == ForgeSlot.Base ? PreviewItem : Selection[slot];
		if (itm is null) return;

		ItemTooltip.ShowTooltip(Services, itm, (PictureBox)sender, true);
	}

	private void pictureBox_MouseLeave(object sender, EventArgs e)
	{
		ItemTooltip.HideTooltip();
	}

	private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
		=> ResetPreviewItem();

	private void scalingCheckBoxHardcore_CheckedChanged(object sender, EventArgs e)
	{
		this.HardcoreMode = scalingCheckBoxHardcore.Checked;
	}

	#endregion

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this.EndDragCursor();
			this.modeToolTip?.Dispose();
			this.panelBackground?.Dispose();
			this.tableBackground?.Dispose();
			this.components?.Dispose();
		}
		base.Dispose(disposing);
	}
}
