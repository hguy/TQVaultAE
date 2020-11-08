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
	public class BagButton : Button
	{
		private Bitmap CurrentImage;

		public BagButton()
		{
			this.BackgroundImage = Resources.inventorybagup01;
			this.CurrentImage = Resources.inventorybagup01;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Size = new System.Drawing.Size(43, 38);
			this.UseVisualStyleBackColor = true;
			this.MouseEnter += new System.EventHandler(this_MouseEnter);
			this.MouseLeave += new System.EventHandler(this_MouseLeave);
		}

		private void this_MouseLeave(object sender, EventArgs e) =>
			this.BackgroundImage = CurrentImage;

		private void this_MouseEnter(object sender, EventArgs e) =>
			this.BackgroundImage = Resources.inventorybagover01;
	}
}
