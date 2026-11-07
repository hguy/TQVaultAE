using System;
using System.Text;
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
			// The save files use Windows-1252 text, which .NET only provides through the code pages provider.
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
