using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Image = iTextSharp.text.Image;

public static class Extensions
{
	public static byte[] ToPdf(this List<System.Drawing.Image> images)
	{
		if (images.Count == 0)
			return null;

		using(MemoryStream ms = new MemoryStream())
		{
			using (Document doc = new Document(PageSize.LETTER, 0, 0, 0, 0))
			{
				PdfWriter writer = PdfWriter.GetInstance(doc, ms);
				doc.Open();

				foreach(System.Drawing.Image image in images)
				{
					doc.NewPage();
					Image jpg = Image.GetInstance(image.ToByteArray());
					jpg.ScalePercent(48f); // 72dpi / 150dpi
					doc.Add(jpg);
				}
			}
			return ms.ToArray();
		}
	}

	public static byte[] ToByteArray(this System.Drawing.Image imageIn)
	{
		MemoryStream ms = new MemoryStream();
		imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
		return ms.ToArray();
	}
}
