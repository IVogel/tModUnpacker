using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace tModUnpacker
{
	static class RawImage
	{
		public static byte[] ToRaw(Bitmap image)
		{
			BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int size = image.Width * image.Height * 4;
			byte[] data = new byte[size];
			Marshal.Copy(bitmapData.Scan0, data, 0, size);
			for (int index = 0; index < size; index = index + 4)
			{
				byte r = data[index    ];
				byte g = data[index + 1];
				byte b = data[index + 2];
				byte a = data[index + 3];

				data[index    ] = b;
				data[index + 1] = g;
				data[index + 2] = r;
				data[index + 3] = a;
			}
			image.UnlockBits(bitmapData);
			return data;
		}

		public static Bitmap RawToPng(byte[] image)
		{
			Stream       stream = new MemoryStream(image);
			BinaryReader reader = new BinaryReader(stream);
			stream.Seek(4, SeekOrigin.Begin);
			int width   = reader.ReadInt32();
			int height  = reader.ReadInt32();
			int size    = width * height * 4;
			byte[] data = new byte[size];

			Bitmap output = new Bitmap(width, height, PixelFormat.Format32bppArgb);

			for (int index = 0; index < size; index = index + 4)
			{
				data[index + 2] = reader.ReadByte();
				data[index + 1] = reader.ReadByte();
				data[index    ] = reader.ReadByte();
				data[index + 3] = reader.ReadByte();
			}
			BitmapData bitmapData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(data, 0, bitmapData.Scan0, size);
			output.UnlockBits(bitmapData);
			return output;
		}
	}
}
