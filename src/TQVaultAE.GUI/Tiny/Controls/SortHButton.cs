using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tiny.Controls
{
	public class SortHButton : Button
	{
		private Bitmap CurrentImage;

		public SortHButton()
		{
			this.BackgroundImage = Resources.autosortup01;
			this.CurrentImage = Resources.autosortup01;
			this.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Size = new System.Drawing.Size(92, 27);
			this.UseVisualStyleBackColor = true;

			this.MouseEnter += new System.EventHandler(this_MouseEnter);
			this.MouseLeave += new System.EventHandler(this_MouseLeave);
		}

		private void this_MouseLeave(object sender, EventArgs e) =>
			this.BackgroundImage = CurrentImage;

		private void this_MouseEnter(object sender, EventArgs e) =>
			this.BackgroundImage = Resources.autosortover01;
	}
}
