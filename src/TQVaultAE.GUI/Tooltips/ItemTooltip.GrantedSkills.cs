using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// The panel of the granted skills, drawn on the right of the item panel as a second tooltip.
/// </summary>
public partial class ItemTooltip
{
	/// <summary>Color made transparent around the two panels, so they look like two tooltips.</summary>
	private static readonly Color SkillsTransparencyKey = Color.FromArgb(255, 0, 255);

	/// <summary>Space between the item panel and the skills panel, in design pixels.</summary>
	private const int SkillsPanelGap = 4;

	/// <summary>Size of a skill icon, in design pixels (the game icons are 32 x 32).</summary>
	private const int SkillIconSize = 32;

	/// <summary>
	/// Builds the skills panel next to the item panel, then shows both as one image with a transparent gap,
	/// and caches that image under <paramref name="key"/>.
	/// </summary>
	private void ShowWithGrantedSkills((Item Item, float Scale, bool AltView, bool InGameValues) key, GrantedSkill[] skills)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var panel = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			BorderStyle = BorderStyle.FixedSingle,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Padding = this.flowLayoutPanelFriendlyNames.Padding,
			Margin = new Padding(0),
		};

		panel.SuspendLayout();
		for (int i = 0; i < skills.Length; i++)
		{
			if (i > 0)
				panel.Controls.Add(MakeRow(this.UIService, this.FontService, TOOLTIPDELIM, BGColor: bg)!);

			this.AddSkillRows(panel, skills[i], bg);
		}
		panel.ResumeLayout();

		// Lay the panel out off the visible area of the form, then draw both panels side by side.
		this.Controls.Add(panel);
		panel.PerformLayout();

		using var itemImage = Rasterize(this.flowLayoutPanelFriendlyNames);
		using var skillsImage = Rasterize(panel);
		this.Controls.Remove(panel);
		panel.Dispose();

		var gap = (int)(SkillsPanelGap * this.UIService.Scale);
		var border = TQColor.DarkGray.Color();
		var composite = new Bitmap(itemImage.Width + skillsImage.Width + gap + 4, Math.Max(itemImage.Height, skillsImage.Height) + 2);
		using (var g = Graphics.FromImage(composite))
		{
			g.Clear(SkillsTransparencyKey);
			DrawFramed(g, itemImage, 0, border);
			DrawFramed(g, skillsImage, itemImage.Width + 2 + gap, border);
		}

		ToImage.Add(key, composite, this.Data!);
		if (ToImage.TryGetCopy(key, out var shown, out _))
			this.ShowSkillsImage(shown);
	}

	/// <summary>Shows an image of the item and skills panels, with the space around them transparent.</summary>
	private void ShowSkillsImage(Bitmap image)
	{
		this.BackColor = SkillsTransparencyKey;
		this.TransparencyKey = SkillsTransparencyKey;
		this.ShowCachedImage(image);
	}

	/// <summary>Draws <paramref name="image"/> at <paramref name="x"/> inside a 1 pixel frame, like the tooltip border.</summary>
	private static void DrawFramed(Graphics g, Bitmap image, int x, Color border)
	{
		using var pen = new Pen(border);
		g.DrawRectangle(pen, x, 0, image.Width + 1, image.Height + 1);
		g.DrawImageUnscaled(image, x + 1, 1);
	}

	private static Bitmap Rasterize(Control control)
	{
		var image = new Bitmap(control.Width, control.Height);
		control.DrawToBitmap(image, new Rectangle(0, 0, control.Width, control.Height));
		return image;
	}

	/// <summary>Icon, name and level, activation, description, then the effects of one skill.</summary>
	private void AddSkillRows(FlowLayoutPanel panel, GrantedSkill skill, Color bg)
	{
		var header = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = new Padding(0, 0, 0, (int)(2 * this.UIService.Scale)),
			Padding = new Padding(0),
		};

		if (skill.Icon is not null)
		{
			var size = (int)(SkillIconSize * this.UIService.Scale);
			header.Controls.Add(new PictureBox()
			{
				Image = this.UIService.LoadBitmap(skill.Icon),
				SizeMode = PictureBoxSizeMode.StretchImage,
				Size = new Size(size, size),
				BackColor = bg,
				Margin = new Padding(0, 0, (int)(4 * this.UIService.Scale), 0),
			});
		}

		var title = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0),
			Anchor = AnchorStyles.Left,
		};
		title.Controls.Add(MakeSingleColorLabel(this.UIService, this.FontService, skill.Name, ItemStyle.Epic.TQColor().Color(), 10F, FontStyle.Bold, bg));
		title.Controls.Add(MakeSingleColorLabel(this.UIService, this.FontService, skill.LevelText, ItemStyle.Broken.TQColor().Color(), 10F, FontStyle.Regular, bg));
		header.Controls.Add(title);
		panel.Controls.Add(header);

		if (skill.Activation is not null)
			AddPanelRow(panel, skill.Activation, ItemStyle.Mundane.TQColor().Color(), bg);

		foreach (var line in skill.Description)
			AddPanelRow(panel, line, ItemStyle.Mundane.TQColor().Color(), bg);

		if (skill.Attributes.Length != 0)
		{
			AddPanelRow(panel, TOOLTIPSPACER, null, bg);
			foreach (var line in skill.Attributes)
				AddPanelRow(panel, line.Length == 0 ? TOOLTIPSPACER : line, null, bg);
		}
	}

	private void AddPanelRow(FlowLayoutPanel panel, string text, Color? color, Color bg)
	{
		if (MakeRow(this.UIService, this.FontService, text, color, BGColor: bg) is { } row)
			panel.Controls.Add(row);
	}
}
