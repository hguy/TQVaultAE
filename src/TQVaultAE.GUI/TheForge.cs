namespace TQVaultAE.GUI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.GUI.Components;
	using TQVaultAE.GUI.Helpers;
	using TQVaultAE.Presentation;
	using TQVaultAE.Domain.Helpers;
	using System.Drawing;
	using Microsoft.Extensions.Logging;
	using TQVaultAE.GUI.Models.SearchDialogAdvanced;
	using TQVaultAE.GUI.Tooltip;
	using Newtonsoft.Json;

	/// <summary>
	/// Class for the Search Dialog box.
	/// </summary>
	public partial class TheForge : VaultForm
	{
		private readonly SessionContext Ctx;
		private readonly ITranslationService TranslationService;
		private readonly List<ItemLocation> ItemDatabase = new List<ItemLocation>();
		private readonly ILogger Log;
		private readonly IItemService ItemService;
		private readonly Bitmap ButtonImageUp;
		private readonly Bitmap ButtonImageDown;

		public TheForge(
			MainForm instance
			, SessionContext sessionContext
			, IItemService itemService
			, ITranslationService translationService
			, ILogger<SearchDialogAdvanced> log
		) : base(instance.ServiceProvider)
		{
			this.Owner = instance;
			this.Ctx = sessionContext;
			this.TranslationService = translationService;
			this.Log = log;
			this.ItemService = itemService;

			this.InitializeComponent();

			this.MinimizeBox = false;
			this.NormalizeBox = false;
			this.MaximizeBox = true;

			#region Apply custom font

			this.ProcessAllControls(c =>
			{
				if (c is IScalingControl || c is NumericUpDown) c.Font = FontService.GetFont(9F);
			});

			this.applyButton.Font = FontService.GetFontLight(12F);
			this.cancelButton.Font = FontService.GetFontLight(12F);

			this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

			#endregion

			#region Load localized strings

			this.Text = Resources.SearchDialogCaption;
			this.applyButton.Text = TranslationService.TranslateXTag("tagMenuButton07");
			this.cancelButton.Text = TranslationService.TranslateXTag("tagMenuButton06");


			#endregion

		}


		#region Apply & Cancel

		/// <summary>
		/// Handler for clicking the apply button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ApplyButtonClicked(object sender, EventArgs e)
		{
			//if (!_SelectedFilters.Any())
			//{
			//	scalingLabelProgress.Text = $"{Resources.SearchTermRequired} - {string.Format(Resources.SearchItemCountIs, ItemDatabase.Count())}";
			//	return;
			//};

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the cancel button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClicked(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion

		/// <summary>
		/// Handler for showing the search dialog.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchDialogShown(object sender, EventArgs e)
		{
			Application.DoEvents();// Force control rendering (VaultForm stuff like custom borders etc...)

			// Init Data Base
			scalingLabelProgress.Text = Resources.SearchBuildingData;
			scalingLabelProgress.Visible = true;

			vaultProgressBar.Minimum = 0;
			vaultProgressBar.Maximum = ItemDatabase.Count();
			vaultProgressBar.Visible = true;

			this.backgroundWorkerBuildDB.RunWorkerAsync();
		}

		#region Load & Init

		private void SearchDialogAdvanced_Load(object sender, EventArgs e)
		{ }

		#endregion

		#region backgroundWorkerBuildDB

		private void backgroundWorkerBuildDB_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{ }

		private void backgroundWorkerBuildDB_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
			=> vaultProgressBar.Increment(1);

		private void backgroundWorkerBuildDB_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
		}


		#endregion

		private void scalingCheckedListBox2_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}

}