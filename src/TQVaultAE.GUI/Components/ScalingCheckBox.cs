//-----------------------------------------------------------------------
// <copyright file="ScalingCheckBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components;

using System;
using System.Drawing;
using System.Windows.Forms;
using CheckBoxState = System.Windows.Forms.VisualStyles.CheckBoxState;

/// <summary>
/// CheckBox class to support scaling of the fonts.
/// </summary>
public class ScalingCheckBox : CheckBox, IScalingControl
{
	/// <summary>
	/// Draws disabled light text dimmed instead of the system dark gray, which is not readable on the dark UI.
	/// Only the standard left-aligned glyph layout is handled; other layouts keep the system rendering.
	/// </summary>
	protected override void OnPaint(PaintEventArgs pevent)
	{
		if (this.Enabled
			|| this.Appearance != Appearance.Normal
			|| this.CheckAlign != ContentAlignment.MiddleLeft
			|| this.RightToLeft == RightToLeft.Yes
			|| this.Image is not null
			|| !DisabledTextPainter.Applies(this.ForeColor)) {
			base.OnPaint(pevent);
			return;
		}

		var g = pevent.Graphics;
		var state = this.CheckState switch {
			CheckState.Checked => CheckBoxState.CheckedDisabled,
			CheckState.Indeterminate => CheckBoxState.MixedDisabled,
			_ => CheckBoxState.UncheckedDisabled,
		};

		// Background and glyph come from the normal rendering; the text area is then repainted with the parent
		// background and the dimmed text drawn over it.
		base.OnPaint(pevent);

		var glyph = CheckBoxRenderer.GetGlyphSize(g, state);
		int textLeft = this.Padding.Left + glyph.Width + TextGap;
		var textBounds = new Rectangle(textLeft, 0, Math.Max(0, this.ClientSize.Width - textLeft), this.ClientSize.Height);
		if (textBounds.Width == 0)
			return;

		ButtonRenderer.DrawParentBackground(g, textBounds, this);

		var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping;
		if (!this.UseMnemonic)
			flags |= TextFormatFlags.NoPrefix;
		if (this.AutoEllipsis)
			flags |= TextFormatFlags.EndEllipsis;
		if (!this.AutoSize)
			flags |= TextFormatFlags.WordBreak;

		TextRenderer.DrawText(g, this.Text, this.Font, textBounds, DisabledTextPainter.ColorFor(this.ForeColor), flags);
	}

	/// <summary>Gap between the glyph and the text, as drawn by the system checkbox.</summary>
	private const int TextGap = 3;

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);

		base.ScaleControl(factor, specified);
	}
}