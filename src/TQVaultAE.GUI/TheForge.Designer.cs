using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.UI;

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
            this.applyButton = new TQVaultAE.GUI.Components.UI.ScalingButton();
            this.cancelButton = new TQVaultAE.GUI.Components.UI.ScalingButton();
            this.vaultProgressBar = new TQVaultAE.GUI.Components.UI.VaultProgressBar();
            this.scalingLabelProgress = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.scalingLabelProgressPanelAlignText = new System.Windows.Forms.Panel();
            this.backgroundWorkerBuildDB = new System.ComponentModel.BackgroundWorker();
            this.typeAssistant = new TQVaultAE.GUI.Components.Behaviors.TypeAssistant();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingComboBox1 = new TQVaultAE.GUI.Components.UI.ScalingComboBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabel2 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.scalingLabel3 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelBlackSmith = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingRadioButton1 = new TQVaultAE.GUI.Components.UI.ScalingRadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.scalingRadioButton2 = new TQVaultAE.GUI.Components.UI.ScalingRadioButton();
            this.scalingRadioButton3 = new TQVaultAE.GUI.Components.UI.ScalingRadioButton();
            this.scalingTextBox1 = new TQVaultAE.GUI.Components.UI.ScalingTextBox();
            this.scalingLabel5 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel9 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabel8 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingCheckedListBox1 = new TQVaultAE.GUI.Components.UI.ScalingCheckedListBox();
            this.flowLayoutPanel10 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabel4 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.scalingCheckedListBox2 = new TQVaultAE.GUI.Components.UI.ScalingCheckedListBox();
            this.scalingLabel6 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.scalingTextBox2 = new TQVaultAE.GUI.Components.UI.ScalingTextBox();
            this.flowLayoutPanel11 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel12 = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabel7 = new TQVaultAE.GUI.Components.UI.ScalingLabel();
            this.flowLayoutPanel13 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanelBottom.SuspendLayout();
            this.scalingLabelProgressPanelAlignText.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel7.SuspendLayout();
            this.flowLayoutPanelBlackSmith.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.flowLayoutPanel9.SuspendLayout();
            this.flowLayoutPanel6.SuspendLayout();
            this.flowLayoutPanel10.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.flowLayoutPanel11.SuspendLayout();
            this.flowLayoutPanel12.SuspendLayout();
            this.flowLayoutPanel13.SuspendLayout();
            this.flowLayoutPanelHeader.SuspendLayout();
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
            this.applyButton.Location = new System.Drawing.Point(29, 36);
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
            this.cancelButton.Location = new System.Drawing.Point(1137, 36);
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
            this.vaultProgressBar.Location = new System.Drawing.Point(198, 31);
            this.vaultProgressBar.Maximum = 0;
            this.vaultProgressBar.Minimum = 0;
            this.vaultProgressBar.Name = "vaultProgressBar";
            this.vaultProgressBar.Size = new System.Drawing.Size(906, 42);
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
            this.scalingLabelProgress.Size = new System.Drawing.Size(43, 17);
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
            this.tableLayoutPanelBottom.Location = new System.Drawing.Point(12, 636);
            this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            this.tableLayoutPanelBottom.RowCount = 2;
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelBottom.Size = new System.Drawing.Size(1304, 71);
            this.tableLayoutPanelBottom.TabIndex = 6;
            // 
            // scalingLabelProgressPanelAlignText
            // 
            this.scalingLabelProgressPanelAlignText.Controls.Add(this.scalingLabelProgress);
            this.scalingLabelProgressPanelAlignText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scalingLabelProgressPanelAlignText.Location = new System.Drawing.Point(198, 3);
            this.scalingLabelProgressPanelAlignText.Name = "scalingLabelProgressPanelAlignText";
            this.scalingLabelProgressPanelAlignText.Size = new System.Drawing.Size(906, 20);
            this.scalingLabelProgressPanelAlignText.TabIndex = 6;
            // 
            // backgroundWorkerBuildDB
            // 
            this.backgroundWorkerBuildDB.WorkerReportsProgress = true;
            this.backgroundWorkerBuildDB.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerBuildDB_DoWork);
            this.backgroundWorkerBuildDB.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerBuildDB_ProgressChanged);
            this.backgroundWorkerBuildDB.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerBuildDB_RunWorkerCompleted);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanelHeader, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel13, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel9, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1304, 605);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.scalingComboBox1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 28);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(234, 31);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // scalingComboBox1
            // 
            this.scalingComboBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.scalingComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingComboBox1.FormattingEnabled = true;
            this.scalingComboBox1.Items.AddRange(new object[] {
            "of the Heros",
            "of the Titans",
            "of the Gods"});
            this.scalingComboBox1.Location = new System.Drawing.Point(3, 3);
            this.scalingComboBox1.Name = "scalingComboBox1";
            this.scalingComboBox1.Size = new System.Drawing.Size(228, 25);
            this.scalingComboBox1.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.scalingLabel2);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(525, 121);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.tableLayoutPanel1.SetRowSpan(this.flowLayoutPanel2, 2);
            this.flowLayoutPanel2.Size = new System.Drawing.Size(384, 369);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // scalingLabel2
            // 
            this.scalingLabel2.AutoSize = true;
            this.scalingLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel2.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel2.Name = "scalingLabel2";
            this.scalingLabel2.Size = new System.Drawing.Size(89, 18);
            this.scalingLabel2.TabIndex = 0;
            this.scalingLabel2.Text = "Current Item";
            // 
            // scalingLabel3
            // 
            this.scalingLabel3.AutoSize = true;
            this.scalingLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel3.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel3.Name = "scalingLabel3";
            this.scalingLabel3.Size = new System.Drawing.Size(61, 18);
            this.scalingLabel3.TabIndex = 0;
            this.scalingLabel3.Text = "Prefixes";
            // 
            // flowLayoutPanel7
            // 
            this.flowLayoutPanel7.Controls.Add(this.flowLayoutPanel10);
            this.flowLayoutPanel7.Controls.Add(this.flowLayoutPanel12);
            this.flowLayoutPanel7.Location = new System.Drawing.Point(3, 235);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(473, 226);
            this.flowLayoutPanel7.TabIndex = 6;
            // 
            // flowLayoutPanelBlackSmith
            // 
            this.flowLayoutPanelBlackSmith.AutoSize = true;
            this.flowLayoutPanelBlackSmith.Controls.Add(this.scalingLabel7);
            this.flowLayoutPanelBlackSmith.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanelBlackSmith.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelBlackSmith.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelBlackSmith.Name = "flowLayoutPanelBlackSmith";
            this.flowLayoutPanelBlackSmith.Size = new System.Drawing.Size(251, 62);
            this.flowLayoutPanelBlackSmith.TabIndex = 7;
            // 
            // scalingRadioButton1
            // 
            this.scalingRadioButton1.AutoSize = true;
            this.scalingRadioButton1.Location = new System.Drawing.Point(12, 23);
            this.scalingRadioButton1.Name = "scalingRadioButton1";
            this.scalingRadioButton1.Size = new System.Drawing.Size(63, 22);
            this.scalingRadioButton1.TabIndex = 3;
            this.scalingRadioButton1.TabStop = true;
            this.scalingRadioButton1.Text = "Keep";
            this.scalingRadioButton1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.scalingTextBox1);
            this.groupBox1.Controls.Add(this.scalingRadioButton3);
            this.groupBox1.Controls.Add(this.scalingRadioButton2);
            this.groupBox1.Controls.Add(this.scalingRadioButton1);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(670, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(449, 63);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ItemSeed";
            // 
            // scalingRadioButton2
            // 
            this.scalingRadioButton2.AutoSize = true;
            this.scalingRadioButton2.Location = new System.Drawing.Point(80, 23);
            this.scalingRadioButton2.Name = "scalingRadioButton2";
            this.scalingRadioButton2.Size = new System.Drawing.Size(105, 22);
            this.scalingRadioButton2.TabIndex = 4;
            this.scalingRadioButton2.TabStop = true;
            this.scalingRadioButton2.Text = "Randomize";
            this.scalingRadioButton2.UseVisualStyleBackColor = true;
            // 
            // scalingRadioButton3
            // 
            this.scalingRadioButton3.AutoSize = true;
            this.scalingRadioButton3.Location = new System.Drawing.Point(190, 23);
            this.scalingRadioButton3.Name = "scalingRadioButton3";
            this.scalingRadioButton3.Size = new System.Drawing.Size(60, 22);
            this.scalingRadioButton3.TabIndex = 5;
            this.scalingRadioButton3.TabStop = true;
            this.scalingRadioButton3.Text = "Input";
            this.scalingRadioButton3.UseVisualStyleBackColor = true;
            // 
            // scalingTextBox1
            // 
            this.scalingTextBox1.Location = new System.Drawing.Point(259, 23);
            this.scalingTextBox1.Name = "scalingTextBox1";
            this.scalingTextBox1.Size = new System.Drawing.Size(171, 24);
            this.scalingTextBox1.TabIndex = 6;
            // 
            // scalingLabel5
            // 
            this.scalingLabel5.AutoSize = true;
            this.scalingLabel5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel5.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel5.Name = "scalingLabel5";
            this.scalingLabel5.Size = new System.Drawing.Size(114, 18);
            this.scalingLabel5.TabIndex = 1;
            this.scalingLabel5.Text = "Suffix properties";
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.flowLayoutPanel6);
            this.flowLayoutPanel5.Controls.Add(this.flowLayoutPanel11);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(473, 226);
            this.flowLayoutPanel5.TabIndex = 8;
            // 
            // flowLayoutPanel9
            // 
            this.flowLayoutPanel9.Controls.Add(this.scalingLabel8);
            this.flowLayoutPanel9.Location = new System.Drawing.Point(916, 121);
            this.flowLayoutPanel9.Name = "flowLayoutPanel9";
            this.tableLayoutPanel1.SetRowSpan(this.flowLayoutPanel9, 2);
            this.flowLayoutPanel9.Size = new System.Drawing.Size(384, 369);
            this.flowLayoutPanel9.TabIndex = 9;
            // 
            // scalingLabel8
            // 
            this.scalingLabel8.AutoSize = true;
            this.scalingLabel8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel8.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel8.Name = "scalingLabel8";
            this.scalingLabel8.Size = new System.Drawing.Size(101, 18);
            this.scalingLabel8.TabIndex = 2;
            this.scalingLabel8.Text = "Reforged Item";
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.AutoSize = true;
            this.flowLayoutPanel6.Controls.Add(this.scalingLabel3);
            this.flowLayoutPanel6.Controls.Add(this.scalingCheckedListBox1);
            this.flowLayoutPanel6.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(126, 218);
            this.flowLayoutPanel6.TabIndex = 1;
            // 
            // scalingCheckedListBox1
            // 
            this.scalingCheckedListBox1.FormattingEnabled = true;
            this.scalingCheckedListBox1.Items.AddRange(new object[] {
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1"});
            this.scalingCheckedListBox1.Location = new System.Drawing.Point(3, 21);
            this.scalingCheckedListBox1.Name = "scalingCheckedListBox1";
            this.scalingCheckedListBox1.Size = new System.Drawing.Size(120, 194);
            this.scalingCheckedListBox1.TabIndex = 1;
            // 
            // flowLayoutPanel10
            // 
            this.flowLayoutPanel10.AutoSize = true;
            this.flowLayoutPanel10.Controls.Add(this.scalingLabel4);
            this.flowLayoutPanel10.Controls.Add(this.scalingCheckedListBox2);
            this.flowLayoutPanel10.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel10.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel10.Name = "flowLayoutPanel10";
            this.flowLayoutPanel10.Size = new System.Drawing.Size(126, 218);
            this.flowLayoutPanel10.TabIndex = 2;
            // 
            // scalingLabel4
            // 
            this.scalingLabel4.AutoSize = true;
            this.scalingLabel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel4.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel4.Name = "scalingLabel4";
            this.scalingLabel4.Size = new System.Drawing.Size(60, 18);
            this.scalingLabel4.TabIndex = 0;
            this.scalingLabel4.Text = "Suffixes";
            // 
            // scalingCheckedListBox2
            // 
            this.scalingCheckedListBox2.FormattingEnabled = true;
            this.scalingCheckedListBox2.Items.AddRange(new object[] {
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1",
            "Prefix Name2",
            "Prefix Name3",
            "Prefix Name1"});
            this.scalingCheckedListBox2.Location = new System.Drawing.Point(3, 21);
            this.scalingCheckedListBox2.Name = "scalingCheckedListBox2";
            this.scalingCheckedListBox2.Size = new System.Drawing.Size(120, 194);
            this.scalingCheckedListBox2.TabIndex = 1;
            this.scalingCheckedListBox2.SelectedIndexChanged += new System.EventHandler(this.scalingCheckedListBox2_SelectedIndexChanged);
            // 
            // scalingLabel6
            // 
            this.scalingLabel6.AutoSize = true;
            this.scalingLabel6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabel6.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel6.Name = "scalingLabel6";
            this.scalingLabel6.Size = new System.Drawing.Size(115, 18);
            this.scalingLabel6.TabIndex = 2;
            this.scalingLabel6.Text = "Prefix properties";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.scalingTextBox2);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(260, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(404, 75);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Craftmanship";
            // 
            // scalingTextBox2
            // 
            this.scalingTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scalingTextBox2.Location = new System.Drawing.Point(3, 20);
            this.scalingTextBox2.Multiline = true;
            this.scalingTextBox2.Name = "scalingTextBox2";
            this.scalingTextBox2.ReadOnly = true;
            this.scalingTextBox2.Size = new System.Drawing.Size(398, 52);
            this.scalingTextBox2.TabIndex = 0;
            this.scalingTextBox2.Text = "Stricte : Basé sur la loottable de l\'objet\r\nLaxiste : Basé sur la loottable des o" +
    "bets du même type\r\nWaky : N\'importe quel objet de votre inventaire.";
            // 
            // flowLayoutPanel11
            // 
            this.flowLayoutPanel11.Controls.Add(this.scalingLabel6);
            this.flowLayoutPanel11.Location = new System.Drawing.Point(135, 3);
            this.flowLayoutPanel11.MinimumSize = new System.Drawing.Size(200, 100);
            this.flowLayoutPanel11.Name = "flowLayoutPanel11";
            this.flowLayoutPanel11.Size = new System.Drawing.Size(200, 100);
            this.flowLayoutPanel11.TabIndex = 3;
            // 
            // flowLayoutPanel12
            // 
            this.flowLayoutPanel12.Controls.Add(this.scalingLabel5);
            this.flowLayoutPanel12.Location = new System.Drawing.Point(135, 3);
            this.flowLayoutPanel12.MinimumSize = new System.Drawing.Size(200, 100);
            this.flowLayoutPanel12.Name = "flowLayoutPanel12";
            this.flowLayoutPanel12.Size = new System.Drawing.Size(200, 100);
            this.flowLayoutPanel12.TabIndex = 2;
            // 
            // scalingLabel7
            // 
            this.scalingLabel7.AutoSize = true;
            this.scalingLabel7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scalingLabel7.Location = new System.Drawing.Point(3, 0);
            this.scalingLabel7.Name = "scalingLabel7";
            this.scalingLabel7.Size = new System.Drawing.Size(245, 25);
            this.scalingLabel7.TabIndex = 1;
            this.scalingLabel7.Text = "Choose your Blacksmith";
            // 
            // flowLayoutPanel13
            // 
            this.flowLayoutPanel13.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel13.Controls.Add(this.flowLayoutPanel7);
            this.flowLayoutPanel13.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel13.Location = new System.Drawing.Point(4, 121);
            this.flowLayoutPanel13.Name = "flowLayoutPanel13";
            this.flowLayoutPanel13.Size = new System.Drawing.Size(476, 465);
            this.flowLayoutPanel13.TabIndex = 1;
            // 
            // flowLayoutPanelHeader
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanelHeader, 3);
            this.flowLayoutPanelHeader.Controls.Add(this.flowLayoutPanelBlackSmith);
            this.flowLayoutPanelHeader.Controls.Add(this.groupBox2);
            this.flowLayoutPanelHeader.Controls.Add(this.groupBox1);
            this.flowLayoutPanelHeader.Location = new System.Drawing.Point(4, 4);
            this.flowLayoutPanelHeader.Name = "flowLayoutPanelHeader";
            this.flowLayoutPanelHeader.Size = new System.Drawing.Size(1218, 110);
            this.flowLayoutPanelHeader.TabIndex = 1;
            // 
            // TheForge
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1328, 719);
            this.Controls.Add(this.tableLayoutPanel1);
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
            this.Text = "The Forge";
            this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SearchDialogAdvanced_Load);
            this.Shown += new System.EventHandler(this.SearchDialogShown);
            this.Controls.SetChildIndex(this.tableLayoutPanelBottom, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanelBottom.ResumeLayout(false);
            this.scalingLabelProgressPanelAlignText.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel7.PerformLayout();
            this.flowLayoutPanelBlackSmith.ResumeLayout(false);
            this.flowLayoutPanelBlackSmith.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.flowLayoutPanel9.ResumeLayout(false);
            this.flowLayoutPanel9.PerformLayout();
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            this.flowLayoutPanel10.ResumeLayout(false);
            this.flowLayoutPanel10.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.flowLayoutPanel11.ResumeLayout(false);
            this.flowLayoutPanel11.PerformLayout();
            this.flowLayoutPanel12.ResumeLayout(false);
            this.flowLayoutPanel12.PerformLayout();
            this.flowLayoutPanel13.ResumeLayout(false);
            this.flowLayoutPanelHeader.ResumeLayout(false);
            this.flowLayoutPanelHeader.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private VaultProgressBar vaultProgressBar;
		private ScalingLabel scalingLabelProgress;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
		private System.ComponentModel.BackgroundWorker backgroundWorkerBuildDB;
		private Components.Behaviors.TypeAssistant typeAssistant;
		private System.Windows.Forms.Panel scalingLabelProgressPanelAlignText;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private ScalingComboBox scalingComboBox1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private ScalingLabel scalingLabel2;
		private ScalingLabel scalingLabel3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelBlackSmith;
		private System.Windows.Forms.GroupBox groupBox1;
		private ScalingTextBox scalingTextBox1;
		private ScalingRadioButton scalingRadioButton3;
		private ScalingRadioButton scalingRadioButton2;
		private ScalingRadioButton scalingRadioButton1;
		private ScalingLabel scalingLabel5;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel9;
		private ScalingLabel scalingLabel8;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
		private ScalingCheckedListBox scalingCheckedListBox1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel10;
		private ScalingLabel scalingLabel4;
		private ScalingCheckedListBox scalingCheckedListBox2;
		private ScalingLabel scalingLabel6;
		private System.Windows.Forms.GroupBox groupBox2;
		private ScalingTextBox scalingTextBox2;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel12;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel11;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel13;
		private ScalingLabel scalingLabel7;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelHeader;
	}
}