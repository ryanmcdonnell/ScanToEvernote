using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using WIA;
using System.Security.Principal;
using System.Security.Permissions;

namespace ScanToEvernote
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Sti sti = new Sti();
			IStillImage stillImage = (IStillImage)sti;
			stillImage.RegisterLaunchApplication("ScanToEvernote", Application.ExecutablePath);

			if (String.IsNullOrEmpty(Properties.Settings.Default.EvernoteDeveloperToken))
				Properties.Settings.Default.EvernoteDeveloperToken = Microsoft.VisualBasic.Interaction.InputBox("What is your Evernote API Token?", "ScanToEvernote", "");

			try
			{
				Scanner scanner = new Scanner();
				byte[] document = scanner.ScanPages().ToPdf();
				if (document != null)
				{
					EvernoteClient en = new EvernoteClient();
					var note = en.NewNote(document, "application/pdf");
				}
			}
			catch (Evernote.EDAM.Error.EDAMUserException ex)
			{
				MessageBox.Show(String.Format("Evernote error: {0}\n\nIs your developer API token invalid?", ex.ErrorCode), "ScanToEvernote", MessageBoxButtons.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "ScanToEvernote", MessageBoxButtons.OK);
			}
		}
	}
}
