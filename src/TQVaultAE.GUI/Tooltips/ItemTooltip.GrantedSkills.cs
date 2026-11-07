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
	/// Draws the skills next to the item panel, in columns no taller than the screen, then shows them with the item panel
	/// as one image with transparent gaps, and caches that image under <paramref name="key"/>.
	/// </summary>
	private void ShowWithGrantedSkills((Item Item, float Scale, bool AltView, bool InGameValues) key, GrantedSkill[] skills)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var scale = this.UIService.Scale;
		var gap = (int)(SkillsPanelGap * scale);
		var pad = this.flowLayoutPanelFriendlyNames.Padding.Left + 1;
		var separator = (int)(9 * scale);

		using var itemImage = Rasterize(this.flowLayoutPanelFriendlyNames);

		// A window cannot be taller than the screen: the skills that do not fit go in another column.
		var screen = Screen.FromControl(this.Owner as Control ?? this).WorkingArea;
		var maxHeight = Math.Max(screen.Height - 2 - (2 * pad), 1);

		var blocks = skills.Select(skill => this.RasterizeSkill(skill, bg)).ToList();
		try
		{
			var columns = new List<List<Bitmap>>();
			int height = 0;
			foreach (var block in blocks)
			{
				if (columns.Count == 0 || height + separator + block.Height > maxHeight)
				{
					columns.Add([]);
					height = -separator;
				}
				columns[^1].Add(block);
				height += separator + block.Height;
			}

			var columnImages = columns.Select(column => DrawColumn(column, bg, pad, separator)).ToList();
			try
			{
				var border = TQColor.DarkGray.Color();
				var width = itemImage.Width + 2 + columnImages.Sum(c => gap + c.Width + 2);
				var composite = new Bitmap(width, Math.Max(itemImage.Height, columnImages.Max(c => c.Height)) + 2);
				using (var g = Graphics.FromImage(composite))
				{
					g.Clear(SkillsTransparencyKey);
					DrawFramed(g, itemImage, 0, border);
					int x = itemImage.Width + 2;
					foreach (var column in columnImages)
					{
						x += gap;
						DrawFramed(g, column, x, border);
						x += column.Width + 2;
					}
				}

				ToImage.Add(key, composite, this.Data!);
			}
			finally
			{
				columnImages.ForEach(c => c.Dispose());
			}
		}
		finally
		{
			blocks.ForEach(b => b.Dispose());
		}

		if (ToImage.TryGetCopy(key, out var shown, out _))
			this.ShowSkillsImage(shown);
	}

	/// <summary>Lays out the rows of one skill on the form and draws them.</summary>
	private Bitmap RasterizeSkill(GrantedSkill skill, Color bg)
	{
		var panel = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Padding = new Padding(0),
			Margin = new Padding(0),
		};

		panel.SuspendLayout();
		this.AddSkillRows(panel, skill, bg);
		panel.ResumeLayout();

		this.Controls.Add(panel);
		panel.PerformLayout();
		try
		{
			return Rasterize(panel);
		}
		finally
		{
			this.Controls.Remove(panel);
			panel.Dispose();
		}
	}

	/// <summary>Stacks skill images in a panel like the item one: padded, with a line between two skills.</summary>
	private static Bitmap DrawColumn(List<Bitmap> blocks, Color bg, int pad, int separator)
	{
		var width = blocks.Max(b => b.Width) + (2 * pad);
		var height = blocks.Sum(b => b.Height) + (separator * (blocks.Count - 1)) + (2 * pad);
		var image = new Bitmap(width, height);
		using var g = Graphics.FromImage(image);
		g.Clear(bg);
		using (var frame = new Pen(Color.Black))
			g.DrawRectangle(frame, 0, 0, width - 1, height - 1);

		using var line = new Pen(TQColor.DarkGray.Color());
		int y = pad;
		for (int i = 0; i < blocks.Count; i++)
		{
			if (i > 0)
			{
				var middle = y + (separator / 2) - 1;
				g.DrawLine(line, pad, middle, width - pad - 1, middle);
				y += separator;
			}
			g.DrawImageUnscaled(blocks[i], pad, y);
			y += blocks[i].Height;
		}

		return image;
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
