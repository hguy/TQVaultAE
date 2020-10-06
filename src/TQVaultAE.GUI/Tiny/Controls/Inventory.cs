using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.GUI.Tiny.Controls
{
	public partial class Inventory : UserControl
	{
		public Inventory()
		{
			InitializeComponent();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, TQColorHelper.TQGolden, ButtonBorderStyle.Solid);
		}
	}
}
