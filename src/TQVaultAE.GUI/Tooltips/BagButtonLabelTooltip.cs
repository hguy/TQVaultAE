﻿using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Tooltips;

public partial class BagButtonLabelTooltip : BaseTooltip
{
	private static object syncObj = new object();

	private static BagButtonLabelTooltip? _Current = null;
	private string _Label = string.Empty;
	private Control? _AnchorControl;
	private readonly Bitmap? bgimg;

	internal Control AnchorControl
	{
		set
		{
			_AnchorControl = value;

			// Move it on top of anchor
			var loc = value.PointToScreen(Point.Empty);

			loc.Y -= this.Size.Height + 5;

			var halfbutton = value.Size.Width / 2;

			loc.X += halfbutton;
			loc.X -= this.Size.Width / 2;

			this.Location = loc;
		}
	}

	internal string Label
	{
		set
		{
			_Label = value;

			this.scalingLabel.Text = _Label;

			// Adjust size to text
			var txtsize = TextRenderer.MeasureText(this._Label, this.scalingLabel.Font, this.Size, TextFormatFlags.SingleLine);// May vary with font

			if (txtsize.Width > this.Width)
				this.Width = txtsize.Width + 25;// +20 for marging

			if (txtsize.Height > this.Height - 25)
				this.Height = txtsize.Height + 20;// +20 for marging

			this.scalingLabel.Visible = true;

		}
	}


#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BagButtonLabelTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private BagButtonLabelTooltip(
		MainForm instance
		, IItemProvider itemProvider
		, IFontService fontService
		, IUIService uiService
		, ITranslationService translationService
	) : base(itemProvider, fontService, uiService, translationService)
	{
		this.Owner = instance;

		InitializeComponent();

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		this.scalingLabel.Font = fontService.GetFont(15F, GraphicsUnit.Point);

		this.bgimg = this.UIService.LoadBitmap(@"INGAMEUI\HEALTHMANAOVERLAY01_NEW.TEX");

		if (bgimg is not null)
			this.Size = new Size(bgimg.Width, bgimg.Height);
		this.BackgroundImage = bgimg;
	}

	// to avoid Mainform lost focus with this.TopMost = false
	protected override bool ShowWithoutActivation => true;

	public static void HideTooltip()
	{
		lock (syncObj)
		{
			if (_Current is not null)
			{
				_Current.Hide();
				_Current.scalingLabel.Visible = false;
			}
		}
	}

	#region Factory

	public static BagButtonLabelTooltip? ShowTooltip(GuiServices services, BagButtonBase anchorControl)
	{
		var label = anchorControl.Sack?.BagButtonIconInfo?.Label;
		if (anchorControl is null) return null;
		if (string.IsNullOrWhiteSpace(label)) return null;

		lock (syncObj)
		{
			HideTooltip();
			if (_Current is null)
				_Current = new BagButtonLabelTooltip(
					services.Forms.MainForm
					, services.ItemProvider
					, services.FontService
					, services.UIService
					, services.TranslationService
				);

			_Current.ClientSize = new Size(187, 40);// Reset Size before computation
			_Current.Label = label;
			_Current.AnchorControl = anchorControl;
			_Current.Show();
		}
		return _Current;
	}

	#endregion


}