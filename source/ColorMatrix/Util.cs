using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Util
{
	#region Imagen

	public static String Number_To_RGBA_Normalized(decimal _Value,
		decimal A_MinValue = 0, decimal A_MaxValue = 1,
		decimal B_MinValue = 0, decimal B_MaxValue = 1,
		int DecimalPlaces = 6)
	{
		string _Ret = string.Empty;
		decimal _RedDec = NormalizeNumber(A_MinValue, A_MaxValue, B_MinValue, B_MaxValue);

		_Ret = string.Format("{0}", Math.Round(_RedDec, DecimalPlaces).ToString().Replace(',', '.').PadRight(DecimalPlaces + 2, '0'));
		return _Ret;
	}

	public static decimal NormalizeNumber(decimal _Value,
		decimal A_MinValue = 0, decimal A_MaxValue = 1,
		decimal B_MinValue = 0, decimal B_MaxValue = 10)
	{
		decimal _ret = 0;
		if (B_MaxValue > B_MinValue && A_MaxValue > A_MinValue)
		{
			_ret = (B_MaxValue - B_MinValue) / (A_MaxValue - A_MinValue) * (_Value - A_MaxValue) + B_MaxValue;
		}
		if (_ret < B_MinValue)
		{
			_ret = B_MinValue;
		}

		if (_ret > B_MaxValue)
		{
			_ret = B_MaxValue;
		}

		return _ret;
	}

	public static decimal Max(params decimal[] values)
	{
		decimal max = values[0];
		for (int i = 1; i < values.Length; i++)
		{
			if (values[i] > max)
			{
				max = values[i];
			}
		}
		return max;
	}
	public static int Max(params int[] values)
	{
		int max = values[0];
		for (int i = 1; i < values.Length; i++)
		{
			if (values[i] > max)
			{
				max = values[i];
			}
		}
		return max;
	}

	public static Image byteArrayToImage(byte[] byteArrayIn)
	{
		Image returnImage = null;
		if (byteArrayIn != null && byteArrayIn.Length > 0)
		{
			using (MemoryStream ms = new MemoryStream(byteArrayIn))
			{
				returnImage = Image.FromStream(ms);
			}
		}
		return returnImage;
	}

	#endregion

	// Perform gamma correction on the image.
	public static Bitmap AdjustGamma(Image image, float gamma = 2.2f)
	{
		// Set the ImageAttributes object's gamma value.
		System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
		attributes.SetGamma(gamma);

		// Draw the image onto the new bitmap
		// while applying the new gamma value.
		Point[] points =
		{
				new Point(0, 0),
				new Point(image.Width, 0),
				new Point(0, image.Height),
			};
		Rectangle rect =
			new Rectangle(0, 0, image.Width, image.Height);

		// Make the result bitmap.
		Bitmap bm = new Bitmap(image.Width, image.Height);
		using (Graphics gr = Graphics.FromImage(bm))
		{
			gr.DrawImage(image, points, rect,
				GraphicsUnit.Pixel, attributes);
		}

		// Return the result.
		return bm;
	}

	public static Image SetSaturation(Image image, float Saturation = 1.0f)
	{
		//saturation = 1f - (saturationTrackBar.Value / 100f);
		Bitmap output = new Bitmap(image.Width, image.Height);

		// Luminance vector for linear RGB
		const float rwgt = 0.3086f;
		const float gwgt = 0.6094f;
		const float bwgt = 0.0820f;

		float baseSat = 1.0f - Saturation;

		System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix();

		colorMatrix[0, 0] = baseSat * rwgt + Saturation;
		colorMatrix[0, 1] = baseSat * rwgt;
		colorMatrix[0, 2] = baseSat * rwgt;
		colorMatrix[1, 0] = baseSat * gwgt;
		colorMatrix[1, 1] = baseSat * gwgt + Saturation;
		colorMatrix[1, 2] = baseSat * gwgt;
		colorMatrix[2, 0] = baseSat * bwgt;
		colorMatrix[2, 1] = baseSat * bwgt;
		colorMatrix[2, 2] = baseSat * bwgt + Saturation;

		System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
		imageAttributes.ClearColorMatrix();
		imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

		using (Graphics g = Graphics.FromImage(output))
		{
			g.DrawImage(image, new Rectangle(0, 0, output.Width, output.Height),
				0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
		}
		return output;
	}


}

