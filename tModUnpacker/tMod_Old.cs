using System;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace tModUnpacker {
	class tMod_Old : tModBase
	{
		public tMod_Old(string path) : base(path) {}

		override public bool ReadMod()
		{
			using (FileStream fileStream = File.OpenRead(this.path))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != "TMOD")
					return false;

				info.modloaderversion = new Version(binaryReader.ReadString());
				info.modhash = binaryReader.ReadBytes(20);
				info.modsignature = binaryReader.ReadBytes(256);
				fileStream.Seek(4, SeekOrigin.Current);

				DeflateStream inflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
				inflateStream.CopyTo(this.tempfile);
				inflateStream.Close();

				this.tempfile.Seek(0, SeekOrigin.Begin);
				BinaryReader tempFileBinaryReader = new BinaryReader(this.tempfile);

				info.modname = tempFileBinaryReader.ReadString();
				info.modversion = new Version(tempFileBinaryReader.ReadString());
				info.filecount = tempFileBinaryReader.ReadInt32();

				for (int index = 0; index < info.filecount; index++)
				{
					tModFileInfo file = new tModFileInfo();
					string path = tempFileBinaryReader.ReadString().Replace("\\", "/");
					file.filesize = tempFileBinaryReader.ReadInt32();
					file.filestart = this.tempfile.Position;
					this.tempfile.Seek(file.filesize, SeekOrigin.Current);
					this.files.Add(path, file);
				}
			}
			return true;
		}

		override public byte[] GetFile(string path)
		{
			path = path.Replace("\\", "/");
			if (HasFile(path))
			{
				tModFileInfo file = this.files[path];
				this.tempfile.Seek(file.filestart, SeekOrigin.Begin);

				byte[] data = data = new byte[file.filesize];
				this.tempfile.Read(data, 0, (int)file.filesize);
				return data;
			}
			return null;
		}

		override public bool DumpFile(string outputpath, string filename)
		{
			string path = Path.Combine(outputpath, this.info.modname, filename);
			byte[] data = this.GetFile(filename);

			if (data == null)
				return false;
			string ext = Path.GetExtension(filename).ToLower();
			if (ext != null && ext == ".rawimg")
			{
				System.Drawing.Bitmap image = RawImage.RawToPng(data);
				MemoryStream stream = new MemoryStream();
				image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				WriteFile(Path.ChangeExtension(path, "png"), stream.ToArray());
				return true;
			}
			WriteFile(path, data);
			return true;
		}
	}
}