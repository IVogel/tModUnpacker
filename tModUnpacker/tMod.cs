using System.IO;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.Collections;
using System;

namespace tModUnpacker
{
	class tMod : tModBase
	{
		public tMod(string path) : base(path) {}

		~tMod()
		{
			if (this.tempfile != null)
				this.tempfile.Close();
		}

		override public byte[] GetFile(string path)
		{
			path = path.Replace("\\", "/");
			if (HasFile(path))
			{
				tModFileInfo file = this.files[path];
				this.tempfile.Seek(file.filestart, SeekOrigin.Begin);

				byte[] data = data = new byte[file.filesize];
				if (file.iscompressed)
				{
					byte[] compresseddata = new byte[file.compressedlen];
					this.tempfile.Read(compresseddata, 0, (int)file.compressedlen);
					Stream stream = new MemoryStream(compresseddata);
					DeflateStream inflateStream = new DeflateStream(stream, CompressionMode.Decompress);
					inflateStream.Read(data, 0, (int)file.filesize);

				} else {
					this.tempfile.Read(data, 0, (int)file.filesize);
				}
				return data;
			}
			return null;
		}

		override public bool ReadMod()
		{
			tModInfo info = new tModInfo();
			using (FileStream fileStream = File.OpenRead(this.path))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != "TMOD")
					return false;

				info.modloaderversion = new Version(binaryReader.ReadString());
				info.modhash          = binaryReader.ReadBytes(20);
				info.modsignature     = binaryReader.ReadBytes(256);
				fileStream.Seek(4, SeekOrigin.Current);

				fileStream.CopyTo(this.tempfile);
				this.tempfile.Seek(0, SeekOrigin.Begin);
				BinaryReader tempFileBinaryReader = new BinaryReader(this.tempfile);
				info.modname = tempFileBinaryReader.ReadString();
				Console.WriteLine(info.modname);
				info.modversion = new Version(tempFileBinaryReader.ReadString());
				info.filecount = tempFileBinaryReader.ReadInt32();
				int WTF = 0;
				IDictionary<string, tModFileInfo> wtfDict = new Dictionary<string, tModFileInfo>();
				for (int index = 0; index < info.filecount; index++)
				{
					tModFileInfo file = new tModFileInfo();
					string path = tempFileBinaryReader.ReadString().Replace("\\", "/");
					file.filesize = tempFileBinaryReader.ReadInt32();
					file.filestart = WTF;
					file.compressedlen = tempFileBinaryReader.ReadInt32();
					WTF += file.compressedlen;
					wtfDict.Add(path, file);
				}
				int datastart = (int)this.tempfile.Position;
				foreach (string fileName in wtfDict.Keys)
				{
					tModFileInfo file = wtfDict[fileName];
					file.filestart += datastart;
					this.files.Add(fileName, file);
				}
				this.info = info;
				return true;
			}
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
				MemoryStream stream         = new MemoryStream();
				image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				WriteFile(Path.ChangeExtension(path, "png"), stream.ToArray());
				return true;
			}
			WriteFile(path, data);
			return true;
		}
	}
}
