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
			using (Document doc = new Document())
			{
				PdfWriter writer = PdfWriter.GetInstance(doc, ms);
				doc.Open();

				foreach(System.Drawing.Image image in images)
				{
                    bool isLetterSized = new Bitmap(image).FitsLetterHeight();
                    if (isLetterSized)
                        doc.SetPageSize(PageSize.LETTER);
                    else
                        doc.SetPageSize(PageSize.LEGAL);
                    
                    doc.SetMargins(0, 0, 0, 0);
					doc.NewPage();
					Image jpg = Image.GetInstance(image.ToByteArray());
					jpg.ScalePercent(48f); // 72dpi / 150dpi
					doc.Add(jpg);
				}
			}
			return ms.ToArray();
		}
	}

    public static bool FitsLetterHeight(this Bitmap bmp)
    {
        // Adapted from http://stackoverflow.com/questions/248141/remove-surrounding-whitespace-from-an-image

        int w = bmp.Width;
        int h = bmp.Height;

        Func<int, bool> allWhiteRow = row =>
        {
            for (int i = 0; i < w; ++i)
            {
                byte red = bmp.GetPixel(i, row).R;
                if (red != 255)
                    return false;
            }
            return true;
        };

        int bottommost = h;
        for (int row = h - 1; row >= 0; --row)
        {
            if (allWhiteRow(row))
                bottommost = row;
            else break;
        }

        return (bottommost <= (1650 + 15)); // 11in @ 150dpi + 10% for crooked pages
    }

	public static byte[] ToByteArray(this System.Drawing.Image imageIn)
	{
		MemoryStream ms = new MemoryStream();
		imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
		return ms.ToArray();
	}
}
