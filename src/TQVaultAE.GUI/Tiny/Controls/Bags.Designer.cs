namespace TQVaultAE.GUI.Tiny.Controls
{
	partial class Bags
	{
		/// <summary> 
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Bags));
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.Bag1 = new TQVaultAE.GUI.Tiny.Controls.Bag();
            this.flpBagButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.BagButton1 = new TQVaultAE.GUI.Tiny.Controls.BagButton();
            this.BagButton2 = new TQVaultAE.GUI.Tiny.Controls.BagButton();
            this.BagButton3 = new TQVaultAE.GUI.Tiny.Controls.BagButton();
            this.SortHButton = new TQVaultAE.GUI.Tiny.Controls.SortHButton();
            this.table.SuspendLayout();
            this.flpBagButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // table
            // 
            this.table.AutoSize = true;
            this.table.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.table.ColumnCount = 2;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.table.Controls.Add(this.Bag1, 0, 1);
            this.table.Controls.Add(this.flpBagButtons, 0, 0);
            this.table.Controls.Add(this.SortHButton, 1, 0);
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Margin = new System.Windows.Forms.Padding(0);
            this.table.Name = "table";
            this.table.RowCount = 2;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 205F));
            this.table.Size = new System.Drawing.Size(268, 245);
            this.table.TabIndex = 0;
            // 
            // Bag1
            // 
            this.Bag1.AutoSize = true;
            this.Bag1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Bag1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.table.SetColumnSpan(this.Bag1, 2);
            this.Bag1.Location = new System.Drawing.Point(0, 40);
            this.Bag1.Margin = new System.Windows.Forms.Padding(0);
            this.Bag1.Name = "Bag1";
            this.Bag1.Padding = new System.Windows.Forms.Padding(1);
            this.Bag1.Size = new System.Drawing.Size(268, 201);
            this.Bag1.TabIndex = 0;
            // 
            // flpBagButtons
            // 
            this.flpBagButtons.AutoSize = true;
            this.flpBagButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpBagButtons.Controls.Add(this.BagButton1);
            this.flpBagButtons.Controls.Add(this.BagButton2);
            this.flpBagButtons.Controls.Add(this.BagButton3);
            this.flpBagButtons.Location = new System.Drawing.Point(0, 0);
            this.flpBagButtons.Margin = new System.Windows.Forms.Padding(0);
            this.flpBagButtons.Name = "flpBagButtons";
            this.flpBagButtons.Size = new System.Drawing.Size(129, 38);
            this.flpBagButtons.TabIndex = 1;
            // 
            // BagButton1
            // 
            this.BagButton1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BagButton1.BackgroundImage")));
            this.BagButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BagButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BagButton1.Location = new System.Drawing.Point(0, 0);
            this.BagButton1.Margin = new System.Windows.Forms.Padding(0);
            this.BagButton1.Name = "BagButton1";
            this.BagButton1.Size = new System.Drawing.Size(43, 38);
            this.BagButton1.TabIndex = 5;
            this.BagButton1.UseVisualStyleBackColor = true;
            // 
            // BagButton2
            // 
            this.BagButton2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BagButton2.BackgroundImage")));
            this.BagButton2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BagButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BagButton2.Location = new System.Drawing.Point(43, 0);
            this.BagButton2.Margin = new System.Windows.Forms.Padding(0);
            this.BagButton2.Name = "BagButton2";
            this.BagButton2.Size = new System.Drawing.Size(43, 38);
            this.BagButton2.TabIndex = 6;
            this.BagButton2.UseVisualStyleBackColor = true;
            // 
            // BagButton3
            // 
            this.BagButton3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BagButton3.BackgroundImage")));
            this.BagButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BagButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BagButton3.Location = new System.Drawing.Point(86, 0);
            this.BagButton3.Margin = new System.Windows.Forms.Padding(0);
            this.BagButton3.Name = "BagButton3";
            this.BagButton3.Size = new System.Drawing.Size(43, 38);
            this.BagButton3.TabIndex = 7;
            this.BagButton3.UseVisualStyleBackColor = true;
            // 
            // SortHButton
            // 
            this.SortHButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SortHButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SortHButton.BackgroundImage")));
            this.SortHButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SortHButton.Location = new System.Drawing.Point(176, 13);
            this.SortHButton.Margin = new System.Windows.Forms.Padding(0);
            this.SortHButton.Name = "SortHButton";
            this.SortHButton.Size = new System.Drawing.Size(92, 27);
            this.SortHButton.TabIndex = 2;
            this.SortHButton.UseVisualStyleBackColor = true;
            // 
            // Bags
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.table);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "Bags";
            this.Size = new System.Drawing.Size(268, 245);
            this.table.ResumeLayout(false);
            this.table.PerformLayout();
            this.flpBagButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel table;
		private Bag Bag1;
		private System.Windows.Forms.FlowLayoutPanel flpBagButtons;
		private BagButton BagButton1;
		private BagButton BagButton2;
		private BagButton BagButton3;
		private SortHButton SortHButton;
	}
}
