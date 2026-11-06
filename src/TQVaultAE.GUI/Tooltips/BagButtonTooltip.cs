using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Tooltips;

public partial class BagButtonTooltip : BaseTooltip
{
	private static Dictionary<BagButtonBase, BagButtonTooltip> ItemTooltipOpened = new Dictionary<BagButtonBase, BagButtonTooltip>();
	/// <summary>Rendered sack summaries, up to 16 MB.</summary>
	private static readonly TooltipImageCache<(SackCollection Sack, float Scale), object?> ToImage = new(16L << 20);

	public BagButtonBase ButtonSack;

	private Rectangle CurrentWorkingArea;
	private int RightSide;
	private int LeftSide;

#if DEBUG
	// For Design Mode
	public BagButtonTooltip() => InitializeComponent();
#endif

	private BagButtonTooltip(MainForm instance, IItemProvider itemProvider, IFontService fontService, IUIService uiService, ITranslationService translationService) : base(itemProvider, fontService, uiService, translationService)
	{
		InitializeComponent();

		this.Owner = instance;

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		this.CurrentWorkingArea = Screen.FromControl(this).WorkingArea;
		// Tooltip can goes multi-columns (especially for bloated Relic Vault)
		this.flowLayoutPanelFriendlyNames.MaximumSize = new Size(this.CurrentWorkingArea.Width, this.CurrentWorkingArea.Height);

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, this.CurrentWorkingArea.Height);
	}

	// to avoid Mainform lost focus with this.TopMost = false
	protected override bool ShowWithoutActivation => true;
	
	public static void InvalidateCache(params SackCollection[] sack)
	{
		sack = sack.Where(s => s != null).ToArray();
		ToImage.RemoveWhere(k => sack.Contains(k.Sack));
	}

	public static void InvalidateCache(params Item[] items)
	{
		items = items.Where(i => i != null).ToArray();
		ToImage.RemoveWhere(k => k.Sack.Intersect(items).Any());
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

	public static BagButtonTooltip ShowTooltip(GuiServices services, BagButtonBase button)
	{
		BagButtonTooltip _Current;
		if (button?.Sack is null) return null;
		lock (ToImage)
		{
			HideTooltip();
			_Current = new BagButtonTooltip(
				services.Forms.MainForm
				, services.ItemProvider
				, services.FontService
				, services.UIService
				, services.TranslationService
			)
			{
				ButtonSack = button
			};
			ItemTooltipOpened.Add(button, _Current);
			_Current.Show();
		}
		return _Current;
	}


	/// <summary>
	/// Init the tooltip content for a sack.  Summarizes the items within the sack
	/// </summary>
	/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
	/// <returns>string to be displayed in the tooltip</returns>
	public void FillSackToolTip()
	{
		var key = (this.ButtonSack.Sack, UIService.Scale);

		// Redraw
		if (ToImage.TryGetCopy(key, out var cached, out _))
		{
			this.ShowCachedImage(cached);
			return;
		}

		this.SuspendLayout();

		this.flowLayoutPanelFriendlyNames.SuspendLayout();

		if (ButtonSack.Sack.IsEmpty)
			AddRow(Resources.VaultGroupBoxEmpty, ItemStyle.Broken.Color());
		else
		{

			var itemlist = ButtonSack.Sack
				.Where(i => !RecordId.IsNullOrEmpty(i.BaseItemId))// skip empty items
				.ToArray();
			var friendlylist = itemlist
				.Select(i => ItemProvider.GetFriendlyNames(i))
				.OrderBy(d => d.FullNameBagTooltipClean)
				.GroupBy(d => d.FullNameBagTooltip)
				.Select(g => new
				{
					FullName = g.Key.InsertAfterColorPrefix($"{g.Count()} x "),
					Data = g.First()
				})
				.ToArray();

			foreach (var item in friendlylist)
				AddRow(item.FullName, item.Data.Item.ExtractTextColorOrItemColor(item.Data.BaseItemInfoDescription));

		}

		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();

		// Rasterize flowLayoutPanelFriendlyNames and cache it for fast redraw
		var w = this.flowLayoutPanelFriendlyNames.Width;
		var h = this.flowLayoutPanelFriendlyNames.Height;
		var raster = new Bitmap(w, h);
		this.flowLayoutPanelFriendlyNames.DrawToBitmap(raster, new Rectangle(0, 0, w, h));
		ToImage.Add(key, raster, null);
	}

	private void AddRow(string friendlyName, Color color)
	{
		Control row = MakeRow(this.UIService, this.FontService, friendlyName, color, 10F, FontStyle.Regular, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);

		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	private void BagButtonTooltip_Load(object sender, EventArgs e)
	{
		this.FillSackToolTip();

		// Move it under BagButton
		var loc = this.ButtonSack.PointToScreen(Point.Empty);
		loc.Y += this.ButtonSack.Size.Height;

		// Ajust position if tooltip size goes offscreen
		var bottom = loc.Y + this.Height;
		if (bottom > this.CurrentWorkingArea.Height)
		{
			// Maximize vertical view
			var offScreenHeight = bottom - this.CurrentWorkingArea.Height;
			if (loc.Y - offScreenHeight < 0)
				loc.Y = 0;// Do your best
			else
				loc.Y -= offScreenHeight;

			this.LeftSide = loc.X - this.Width;

			// Put tooltip on right side of button to avoid mouse pointer overlap
			loc.X += Convert.ToInt32(this.ButtonSack.Size.Width * UIService.Scale);

			this.RightSide = loc.X;

			// Capture mouse move when overing the button to allow dynamic tooltip placement on left<->right
			this.ButtonSack.MouseMove += ButtonSack_MouseMove;
		}

		this.Location = loc;
	}

	private void ButtonSack_MouseMove(object sender, MouseEventArgs e)
	{
		var loc = this.Location;

		if (e.Location.X > this.ButtonSack.Width / 2)
			loc.X = this.RightSide;
		else
			loc.X = this.LeftSide;

		this.Location = loc;
	}

	private void BagButtonTooltip_FormClosing(object sender, FormClosingEventArgs e) => this.ButtonSack.MouseMove -= ButtonSack_MouseMove;
}