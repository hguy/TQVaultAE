using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// SearchDialog Designer class
	/// </summary>
	public partial class TheForge
	{

		/// <summary>
		/// Windows Form Find Button.
		/// </summary>
		private ScalingButton applyButton;

		/// <summary>
		/// Windows Form Cancel Button.
		/// </summary>
		private ScalingButton cancelButton;

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
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TheForge));
            this.applyButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.cancelButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.vaultProgressBar = new TQVaultAE.GUI.Components.VaultProgressBar();
            this.scalingLabelProgress = new TQVaultAE.GUI.Components.ScalingLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.scalingLabelProgressPanelAlignText = new System.Windows.Forms.Panel();
            this.backgroundWorkerBuildDB = new System.ComponentModel.BackgroundWorker();
            this.typeAssistant = new TQVaultAE.GUI.Components.TypeAssistant();
            this.tableLayoutPanelBottom.SuspendLayout();
            this.scalingLabelProgressPanelAlignText.SuspendLayout();
            this.SuspendLayout();
            // 
            // applyButton
            // 
            this.applyButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.applyButton.BackColor = System.Drawing.Color.Transparent;
            this.applyButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.DownBitmap")));
            this.applyButton.FlatAppearance.BorderSize = 0;
            this.applyButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.applyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.applyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.Image = ((System.Drawing.Image)(resources.GetObject("applyButton.Image")));
            this.applyButton.Location = new System.Drawing.Point(35, 36);
            this.applyButton.Name = "applyButton";
            this.applyButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.OverBitmap")));
            this.applyButton.Size = new System.Drawing.Size(137, 30);
            this.applyButton.SizeToGraphic = false;
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.UpBitmap")));
            this.applyButton.UseCustomGraphic = true;
            this.applyButton.UseVisualStyleBackColor = false;
            this.applyButton.Click += new System.EventHandler(this.ApplyButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(1217, 36);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
            this.cancelButton.Size = new System.Drawing.Size(137, 30);
            this.cancelButton.SizeToGraphic = false;
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
            this.cancelButton.UseCustomGraphic = true;
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
            // 
            // vaultProgressBar
            // 
            this.vaultProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vaultProgressBar.BackColor = System.Drawing.Color.Transparent;
            this.vaultProgressBar.Location = new System.Drawing.Point(211, 31);
            this.vaultProgressBar.Maximum = 0;
            this.vaultProgressBar.Minimum = 0;
            this.vaultProgressBar.Name = "vaultProgressBar";
            this.vaultProgressBar.Size = new System.Drawing.Size(967, 42);
            this.vaultProgressBar.TabIndex = 4;
            this.vaultProgressBar.Value = 0;
            // 
            // scalingLabelProgress
            // 
            this.scalingLabelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingLabelProgress.BackColor = System.Drawing.Color.Transparent;
            this.scalingLabelProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelProgress.ForeColor = System.Drawing.Color.DarkOrange;
            this.scalingLabelProgress.Location = new System.Drawing.Point(422, 1);
            this.scalingLabelProgress.Name = "scalingLabelProgress";
            this.scalingLabelProgress.Size = new System.Drawing.Size(104, 17);
            this.scalingLabelProgress.TabIndex = 5;
            this.scalingLabelProgress.Text = "Building Data...";
            this.scalingLabelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.scalingLabelProgress.UseMnemonic = false;
            // 
            // tableLayoutPanelBottom
            // 
            this.tableLayoutPanelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelBottom.ColumnCount = 3;
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelBottom.Controls.Add(this.vaultProgressBar, 1, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.applyButton, 0, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.cancelButton, 2, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.scalingLabelProgressPanelAlignText, 1, 0);
            this.tableLayoutPanelBottom.Location = new System.Drawing.Point(12, 672);
            this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            this.tableLayoutPanelBottom.RowCount = 2;
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelBottom.Size = new System.Drawing.Size(1391, 71);
            this.tableLayoutPanelBottom.TabIndex = 6;
            // 
            // scalingLabelProgressPanelAlignText
            // 
            this.scalingLabelProgressPanelAlignText.Controls.Add(this.scalingLabelProgress);
            this.scalingLabelProgressPanelAlignText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scalingLabelProgressPanelAlignText.Location = new System.Drawing.Point(211, 3);
            this.scalingLabelProgressPanelAlignText.Name = "scalingLabelProgressPanelAlignText";
            this.scalingLabelProgressPanelAlignText.Size = new System.Drawing.Size(967, 20);
            this.scalingLabelProgressPanelAlignText.TabIndex = 6;
            // 
            // backgroundWorkerBuildDB
            // 
            this.backgroundWorkerBuildDB.WorkerReportsProgress = true;
            this.backgroundWorkerBuildDB.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerBuildDB_DoWork);
            this.backgroundWorkerBuildDB.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerBuildDB_ProgressChanged);
            this.backgroundWorkerBuildDB.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerBuildDB_RunWorkerCompleted);

            // 
            // TheForge
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1415, 755);
            this.Controls.Add(this.tableLayoutPanelBottom);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TheForge";
            this.ResizeCustomAllowed = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search for an Item";
            this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.TopMost = true;

            this.Load += new System.EventHandler(this.SearchDialogAdvanced_Load);
            this.Shown += new System.EventHandler(this.SearchDialogShown);
            this.Controls.SetChildIndex(this.tableLayoutPanelBottom, 0);
            this.tableLayoutPanelBottom.ResumeLayout(false);
            this.scalingLabelProgressPanelAlignText.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private VaultProgressBar vaultProgressBar;
		private ScalingLabel scalingLabelProgress;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
		private System.ComponentModel.BackgroundWorker backgroundWorkerBuildDB;
		private Components.TypeAssistant typeAssistant;
		private System.Windows.Forms.Panel scalingLabelProgressPanelAlignText;
	}
}