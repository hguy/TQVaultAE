//-----------------------------------------------------------------------
// <copyright file="StashPanel.PlayerInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for handling the stash panel ui functions
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Rendering;

public partial class StashPanel
{
	/// <summary>
	/// Calculates the stat bonuses for the equipped gear and updates the player
	/// </summary>
	private void GetSkillStatBonus()
	{
		if (Database == null || Player == null)
			return;

		SortedList<string, int> statBonuses = new SortedList<string, int>();

		if (Player.PlayerInfo is null)
			return;

		foreach (SkillRecord skill in Player.PlayerInfo.SkillRecordList)
			ItemProvider.GetStatBonusesFromRecord(statBonuses, Database.GetRecordFromFile(skill.skillName), skill.skillLevel);


		Player.ClearPlayerSkillBonuses();
		Player.UpdatePlayerSkillBonuses(statBonuses);
	}

	private void DisplayPlayerInfo()
	{
		if (this.Player == null || !this.equipmentPanel.Visible || this.BagButtons[this.CurrentBag].ButtonText != Resources.StashPanelBtnEquipment)
		{
			this.PlayerPanel.Visible = false;
			return;
		}

		this.PlayerPanel.Visible = true;

		if (DisplayPlayerInfoLastName == this.Player.PlayerName) return;

		this.SuspendLayout();
		this.PlayerPanel.SuspendLayout();

		this.PlayerPanel.Visible = false;
		this.PlayerPanel.Controls.Clear();

		if (this.Player?.PlayerInfo != null)
		{
			DisplayPlayerInfoLastName = this.Player.PlayerName;
			var pclass = TranslationService.TranslateXTag(this.Player.PlayerInfo.Class ?? string.Empty, true, true) ?? string.Empty;
			var mclass = TranslationService.TranslateMastery(this.Player.PlayerInfo.Class ?? string.Empty) ?? string.Empty;
			mclass = mclass == Resources.Masteries ? string.Empty : mclass;

			var pi = new Dictionary<string, string>
			{
				[Resources.CurrentLevel] = this.Player.PlayerInfo.CurrentLevel.ToString(),
				[Resources.Class] = pclass,
				[Resources.Masteries] = mclass,
				[Resources.CurrentXP] = this.Player.PlayerInfo.CurrentXP.ToString(),
				[Resources.DifficultyUnlocked] = TranslationService.TranslateDifficulty(this.Player.PlayerInfo.DifficultyUnlocked) ?? "unknown",
				[Resources.Money] = this.Player.PlayerInfo.Money.ToString(),
				[Resources.SkillPoints] = this.Player.PlayerInfo.SkillPoints.ToString(),
				[Resources.AttributesPoints] = this.Player.PlayerInfo.AttributesPoints.ToString(),
				////[Resources.BaseStrength] = this.Player.PlayerInfo.BaseStrength.ToString(),
				[Resources.CEStrength] = this.Player.PlayerInfo.CalculatedStrength.ToString(),
				////[Resources.BaseDexterity] = this.Player.PlayerInfo.BaseDexterity.ToString(),
				[Resources.CEDexterity] = this.Player.PlayerInfo.CalculatedDexterity.ToString(),
				////[Resources.BaseIntelligence] = this.Player.PlayerInfo.BaseIntelligence.ToString(),
				[Resources.CEIntelligence] = this.Player.PlayerInfo.CalculatedIntelligence.ToString(),
				////[Resources.BaseHealth] = this.Player.PlayerInfo.BaseHealth.ToString(),
				[Resources.CEHealth] = this.Player.PlayerInfo.CalculatedHealth.ToString(),
				////[Resources.BaseMana] = this.Player.PlayerInfo.BaseMana.ToString(),
				[Resources.CEMana] = this.Player.PlayerInfo.CalculatedMana.ToString(),
				[Resources.PlayTimeInSeconds] = this.Player.PlayerInfo.PlayTimeInSeconds.ToString(),
				[Resources.NumberOfDeaths] = this.Player.PlayerInfo.NumberOfDeaths.ToString(),
				[Resources.NumberOfKills] = this.Player.PlayerInfo.NumberOfKills.ToString(),
				[Resources.ExperienceFromKills] = this.Player.PlayerInfo.ExperienceFromKills.ToString(),
				[Resources.HealthPotionsUsed] = this.Player.PlayerInfo.HealthPotionsUsed.ToString(),
				[Resources.ManaPotionsUsed] = this.Player.PlayerInfo.ManaPotionsUsed.ToString(),
				[Resources.NumHitsReceived] = this.Player.PlayerInfo.NumHitsReceived.ToString(),
				[Resources.NumHitsInflicted] = this.Player.PlayerInfo.NumHitsInflicted.ToString(),
				[Resources.CriticalHitsInflicted] = this.Player.PlayerInfo.CriticalHitsInflicted.ToString(),
				[Resources.CriticalHitsReceived] = this.Player.PlayerInfo.CriticalHitsReceived.ToString(),
			};

			var labStyle = BorderStyle.None;
			var mg = new Padding(0, 0, 0, 1);
			var pd = new Padding(0);
			int rowIdx = 0;
			var labFnt = new Font(this.Font.FontFamily, 7.5f * UIService.Zoom);

			// Add Edit Button
			var editButton = new ScalingButton()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = Color.Transparent,
				DialogResult = DialogResult.OK,
				DownBitmap = Images.MainButtonDown,
				FlatStyle = FlatStyle.Flat,
				ForeColor = Theme.ButtonText,
				Image = Images.MainButtonUp,
				Name = "editButton",
				OverBitmap = Images.MainButtonOver,
				Size = new Size(137, 30),
				SizeToGraphic = false,
				Text = Resources.CharacterEditBtn,
				UpBitmap = Images.MainButtonUp,
				UseCustomGraphic = true,
				UseVisualStyleBackColor = false,
				Font = FontService.GetFontLight(11F),
				Margin = new Padding(0, 0, 0, 5),
				Padding = new Padding(10, 0, 10, 0),
				Visible = this.USettings.AllowCharacterEdit,
			};
			editButton.FlatAppearance.BorderSize = 0;
			editButton.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
			editButton.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
			editButton.Click += EditButton_Click;

			this.PlayerPanel.SetColumnSpan(editButton, 2);
			this.PlayerPanel.Controls.Add(editButton, 0, rowIdx++);

			// Add Player Data
			foreach (var row in pi)
			{
				// Add Row
				this.PlayerPanel.Controls.Add(new ScalingLabel()
				{
					Margin = mg,
					Padding = pd,
					BorderStyle = labStyle,
					AutoSize = true,
					TextAlign = ContentAlignment.TopRight,
					Anchor = AnchorStyles.Top | AnchorStyles.Right,
					ForeColor = Color.White,
					Font = labFnt,
					Text = row.Key
				}, 0, rowIdx);
				this.PlayerPanel.Controls.Add(new ScalingLabel()
				{
					Margin = mg,
					Padding = pd,
					BorderStyle = labStyle,
					AutoSize = true,
					TextAlign = ContentAlignment.TopLeft,
					Anchor = AnchorStyles.Top | AnchorStyles.Left,
					ForeColor = Color.YellowGreen,
					Font = labFnt,
					Text = row.Value
				}, 1, rowIdx);

				rowIdx++;
			}
		}

		this.PlayerPanel.ResumeLayout();
		this.ResumeLayout();

		this.PlayerPanel.Visible = true;
	}
}
