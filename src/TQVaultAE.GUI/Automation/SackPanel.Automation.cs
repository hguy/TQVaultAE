//-----------------------------------------------------------------------
// <copyright file="SackPanel.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// Debug-only entry points used by the automation endpoint.
/// They feed the panel's own mouse handlers with synthetic events and a <see cref="VirtualCursor"/>,
/// so the regular pick / drag / drop logic runs without touching the real mouse.
/// </summary>
public partial class SackPanel
{
	internal ItemDragInfo AutomationDragInfo => this.DragInfo;

	/// <summary>
	/// Gets the cells the dragged item would be dropped into, as computed by the last paint.
	/// </summary>
	internal Rectangle AutomationCellsUnderDragItem => this.CellsUnderDragItem;

	internal Item? AutomationFindItem(Point cell) => this.FindItem(cell);

	/// <summary>
	/// Converts a cell coordinate into the client coordinate of the cell's top left corner.
	/// </summary>
	internal Point AutomationCellTopLeft(Point cell) => this.CellTopLeft(cell);

	/// <summary>
	/// Converts a cell coordinate into the client coordinate of the cell's center.
	/// </summary>
	internal Point AutomationCellCenter(Point cell)
	{
		var topLeft = this.CellTopLeft(cell);
		return new Point(topLeft.X + this.UIService.HalfUnitSize, topLeft.Y + this.UIService.HalfUnitSize);
	}

	/// <summary>
	/// Moves the virtual cursor over this panel and runs the regular mouse move logic,
	/// then paints synchronously so that the drop target cells are up to date.
	/// </summary>
	internal void AutomationMouseMove(Point clientLocation)
	{
		this.AutomationEnter(clientLocation);
		this.MouseMoveCallback(this, new MouseEventArgs(MouseButtons.None, 0, clientLocation.X, clientLocation.Y, 0));
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Simulates a mouse button press at a client location.
	/// </summary>
	internal void AutomationMouseDown(Point clientLocation, MouseButtons button)
	{
		this.AutomationMouseMove(clientLocation);
		var args = new MouseEventArgs(button, 1, clientLocation.X, clientLocation.Y, 0);
		this.MouseDownCallback(this, args);
		this.MouseUpCallback(this, args);
		this.AutomationPaintNow();
	}

	/// <summary>
	/// Simulates the virtual cursor leaving this panel.
	/// </summary>
	internal void AutomationMouseLeave()
	{
		this.MouseLeaveCallback(this, EventArgs.Empty);
		this.Invalidate();
	}

	/// <summary>
	/// Runs the paint logic now, even when the window is minimized or hidden behind other windows.
	/// </summary>
	internal void AutomationPaintNow()
	{
		var form = this.FindForm();
		if (this.Visible && form is not null && form.WindowState != FormWindowState.Minimized)
		{
			this.Refresh();
			return;
		}

		if (this.Width <= 0 || this.Height <= 0)
			return;

		// WM_PRINT goes through the same paint callback and refreshes the drag state.
		using var scratch = new Bitmap(this.Width, this.Height);
		this.DrawToBitmap(scratch, new Rectangle(Point.Empty, this.Size));
	}

	private void AutomationEnter(Point clientLocation)
	{
		if (VirtualCursor.Target is SackPanel previous && !ReferenceEquals(previous, this))
		{
			VirtualCursor.Set(this, clientLocation);
			previous.AutomationMouseLeave();
			previous.AutomationPaintNow();
			return;
		}

		VirtualCursor.Set(this, clientLocation);
	}
}
