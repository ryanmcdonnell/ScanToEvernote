using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using WIA;
using System.Security.Principal;
using System.Security.Permissions;
using System.Threading;
using System.ComponentModel;

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
			if (args.Length > 0 && args[0] == "register")
			{
				try
				{
					Sti sti = new Sti();
					IStillImage stillImage = (IStillImage)sti;
					stillImage.RegisterLaunchApplication("ScanToEvernote", Application.ExecutablePath);
                    TopMostMessageBox.Show("Succesfully registered as scanner button event.", "ScanToEvernote", MessageBoxButtons.OK);
				}
				catch (Exception ex)
				{
                    TopMostMessageBox.Show(String.Format("Error registering as scanner button event: {0}\n\nAre you an administrator?", ex.Message), "ScanToEvernote", MessageBoxButtons.OK);
				}

				return;
			}

			if (String.IsNullOrEmpty(Properties.Settings.Default.EvernoteDeveloperToken))
				Properties.Settings.Default.EvernoteDeveloperToken = Microsoft.VisualBasic.Interaction.InputBox("What is your Evernote API Token?", "ScanToEvernote", "");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			RunBackgroundWorker();

			Application.Run(new ProgressForm());
		}

		private static void RunBackgroundWorker()
		{
			// this is your presumably long-running method
			Action exec = ScanImagesAndUpload;

			BackgroundWorker b = new BackgroundWorker();

			// set the worker to call your long-running method
			b.DoWork += (object sender, DoWorkEventArgs e) =>
			{
				exec.Invoke();
			};

			// this only tells your BackgroundWorker to START working;
			// the current (i.e., GUI) thread will immediately continue,
			// which means your progress bar will update, the window
			// will continue firing button click events and all that
			// good stuff
			b.RunWorkerAsync();
		}

		private static void ScanImagesAndUpload()
		{
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
                TopMostMessageBox.Show(String.Format("Evernote error: {0}\n\nIs your developer API token invalid?", ex.ErrorCode), "ScanToEvernote", MessageBoxButtons.OK);
			}
			catch (Exception ex)
			{
                TopMostMessageBox.Show(ex.Message, "ScanToEvernote", MessageBoxButtons.OK);
			}

			Application.Exit();
		}
	}
}
