using TQ.SaveFilesExplorer.Entities;
using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using System;
using System.Windows.Forms;

namespace TQ.SaveFilesExplorer
{
	static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
