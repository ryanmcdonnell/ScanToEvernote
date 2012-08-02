using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using WIA;

namespace ScanToEvernote
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Scanner scanner = new Scanner();
			byte[] document = scanner.ScanPages().ToPdf();
			if (document != null)
			{
				EvernoteClient en = new EvernoteClient();
				var note = en.NewNote(document, "application/pdf");
			}
		}
	}
}
