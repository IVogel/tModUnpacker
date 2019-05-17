using System.IO;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.Collections;

namespace tModUnpacker
{
	struct tModFileInfo
	{
		public long filestart;
		public long filesize;
	}

	struct tModFile
	{
		public string path;
		public long   size;
		public byte[] data;
	}

	struct tModInfo
	{
		public string modloaderversion;
		public string modversion;
		public string modname;
		public byte[] modhash;
		public byte[] modsignature;
		public int    filecount;
	}

	class tMod : IEnumerable<tModFile>, IEnumerable
	{
		public tModInfo info;

		public IDictionary<string, tModFileInfo> files = new Dictionary<string, tModFileInfo>();

		private string path;

		private FileStream tempfile;
		private string tempfilepath = Path.GetTempFileName();

		internal tMod(string path)
		{
			this.tempfile = new FileStream(
				this.tempfilepath,
				FileMode.OpenOrCreate,
				FileAccess.ReadWrite,
				FileShare.Read,
				4096,
				FileOptions.DeleteOnClose | FileOptions.RandomAccess
			);
			this.path = path;
		}

		~tMod()
		{
			if (this.tempfile != null)
				this.tempfile.Close();
		}

		public bool HasFile(string path)
		{
			return this.files.ContainsKey(path.Replace("\\", "/"));
		}

		public byte[] GetFile(string path)
		{
			path = path.Replace("\\", "/");
			if (HasFile(path))
			{
				tModFileInfo file = this.files[path];
				this.tempfile.Seek(file.filestart, SeekOrigin.Begin);
				byte[] data = new byte[file.filesize];
				this.tempfile.Read(data, 0, (int)file.filesize);

				return data;
			}
			return null;
		}

		public void WriteFile(string path, byte[] data)
		{
			string dirpath = Path.GetDirectoryName(path);
			if (!Directory.Exists(dirpath))
			{
				Directory.CreateDirectory(dirpath);
			}
			File.WriteAllBytes(path, data);
		}

		public bool ReadMod()
		{
			tModInfo info = new tModInfo();
			using (FileStream fileStream = File.OpenRead(this.path))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != "TMOD")
					return false;

				info.modloaderversion = binaryReader.ReadString();
				info.modhash          = binaryReader.ReadBytes(20);
				info.modsignature     = binaryReader.ReadBytes(256);

				fileStream.Seek(4, SeekOrigin.Current);

				DeflateStream inflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
				inflateStream.CopyTo(this.tempfile);
				inflateStream.Close();
				this.tempfile.Seek(0, SeekOrigin.Begin);
				BinaryReader tempFileBinaryReader = new BinaryReader(this.tempfile);

				info.modname    = tempFileBinaryReader.ReadString();
				info.modversion = tempFileBinaryReader.ReadString();
				info.filecount  = tempFileBinaryReader.ReadInt32();
				for (int index = 0; index < info.filecount; index++)
				{
					tModFileInfo file  = new tModFileInfo();
					string path        = tempFileBinaryReader.ReadString().Replace("\\", "/");
					file.filesize      = tempFileBinaryReader.ReadInt32();
					file.filestart     = this.tempfile.Position;
					this.tempfile.Seek(file.filesize, SeekOrigin.Current);
					this.files.Add(path, file);
				}
				this.info = info;
				return true;
			}
		}

		public bool DumpFile(string outputpath, string filename)
		{
			string path = Path.Combine(outputpath, this.info.modname, filename);
			byte[] data = this.GetFile(filename);

			if (data == null)
				return false;

			WriteFile(path, data);
			return true;
		}

		public void DumpFiles(string outputpath)
		{
			foreach (string fileName in this.files.Keys)
			{
				DumpFile(outputpath, fileName);
			}
		}

		public void DumpFiles(string outputpath, System.Action<string> func)
		{
			foreach (string fileName in this.files.Keys)
			{
				func(fileName);
				DumpFile(outputpath, fileName);
			}
		}

		public void DumpFiles(string outputpath, System.Func<string, bool> func)
		{
			foreach (string fileName in this.files.Keys)
			{
				if (!func(fileName))
					continue;
				DumpFile(outputpath, fileName);
			}
		}

		public IEnumerator<tModFile> GetEnumerator()
		{
			foreach (string path in this.files.Keys)
			{
				tModFile file = new tModFile();
				file.path = path;
				file.data = this.GetFile(path);
				file.size = file.data != null ? file.data.Length : 0;
				yield return file;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
