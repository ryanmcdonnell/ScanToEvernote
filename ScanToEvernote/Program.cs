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
			List<Image> images = scanner.ScanPages();
			images.SaveImagesAsNewNote();
		}
	}
}
