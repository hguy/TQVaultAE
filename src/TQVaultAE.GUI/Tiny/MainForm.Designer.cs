namespace TQVaultAE.GUI.Tiny
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.bags1 = new TQVaultAE.GUI.Tiny.Controls.Bags();
            this.SuspendLayout();
            // 
            // bags1
            // 
            this.bags1.AutoSize = true;
            this.bags1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bags1.BackColor = System.Drawing.Color.Transparent;
            this.bags1.Location = new System.Drawing.Point(448, 106);
            this.bags1.Margin = new System.Windows.Forms.Padding(0);
            this.bags1.Name = "bags1";
            this.bags1.Size = new System.Drawing.Size(268, 245);
            this.bags1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1186, 768);
            this.Controls.Add(this.bags1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Tiny Vault AE";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Controls.Bags bags1;
	}
}