using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDHM_UI_Thumbnail_Maker
{
	public static class Util
	{
		/// <summary>Redimensiona una imagen al tamaño deseado.</summary>
		/// <param name="imgToResize">Imagen Original</param>
		/// <param name="size">Tamaño Nuevo</param>
		public static Image ResizeImage(Image imgToResize, Size size)
		{
			Bitmap b = null;
			try
			{
				int sourceWidth = imgToResize.Width;
				int sourceHeight = imgToResize.Height;

				float nPercent = 0;
				float nPercentW = 0;
				float nPercentH = 0;

				nPercentW = ((float)size.Width / (float)sourceWidth);
				nPercentH = ((float)size.Height / (float)sourceHeight);

				if (nPercentH < nPercentW)
					nPercent = nPercentH;
				else
					nPercent = nPercentW;

				int destWidth = (int)(sourceWidth * nPercent);
				int destHeight = (int)(sourceHeight * nPercent);

				b = new Bitmap(destWidth, destHeight);
				Graphics g = Graphics.FromImage((Image)b);
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

				g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
				g.Dispose();
			}
			catch { throw; }
			return (Image)b;
		}

		/// <summary>Redimensiona una imagen al tamaño deseado conservando las proporciones.</summary>
		/// <param name="image">Imagen Original</param>
		/// <param name="height">Tamaño Nuevo</param>
		public static Image ScaleImage(Image image, int height)
		{
			double ratio = (double)height / image.Height;
			int newWidth = (int)(image.Width * ratio);
			int newHeight = (int)(image.Height * ratio);
			Bitmap newImage = new Bitmap(newWidth, newHeight);
			using (Graphics g = Graphics.FromImage(newImage))
			{
				g.DrawImage(image, 0, 0, newWidth, newHeight);
			}
			image.Dispose();
			return newImage;
		}

		public static Bitmap CropImage(Image source, int x, int y, int width, int height)
		{
			Rectangle crop = new Rectangle(x, y, width, height);

			var bmp = new Bitmap(crop.Width, crop.Height);
			using (var gr = Graphics.FromImage(bmp))
			{
				gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
			}
			return bmp;
		}


		/// <summary>
		/// Retrieves the Encoder Information for a given MimeType
		/// </summary>
		/// <param name="mimeType">String: Mimetype</param>
		/// <returns>ImageCodecInfo: Mime info or null if not found</returns>
		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			var encoders = ImageCodecInfo.GetImageEncoders();
			return encoders.FirstOrDefault(t => t.MimeType == mimeType);
		}

		/// <summary>
		/// Save an Image as a JPeg with a given compression
		///  Note: Filename suffix will not affect mime type which will be Jpeg.
		/// </summary>
		/// <param name="image">Image: Image to save</param>
		/// <param name="fileName">String: File name to save the image as. Note: suffix will not affect mime type which will be Jpeg.</param>
		/// <param name="compression">Long: Value between 0 and 100.</param>
		private static void SaveJpegWithCompressionSetting(Image image, string fileName, long compression)
		{
			var eps = new EncoderParameters(1);
			eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);
			var ici = GetEncoderInfo("image/jpeg");
			image.Save(fileName, ici, eps);
		}

		/// <summary>
		/// Save an Image as a JPeg with a given compression
		/// Note: Filename suffix will not affect mime type which will be Jpeg.
		/// </summary>
		/// <param name="image">Image: This image</param>
		/// <param name="fileName">String: File name to save the image as. Note: suffix will not affect mime type which will be Jpeg.</param>
		/// <param name="compression">Long: Value between 0 and 100.</param>
		public static void SaveJpegWithCompression(this Image image, string fileName, long compression)
		{
			SaveJpegWithCompressionSetting(image, fileName, compression);
		}

	}
}
