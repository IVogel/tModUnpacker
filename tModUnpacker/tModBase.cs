using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace tModUnpacker
{
	struct tModFileInfo
	{
		public long filestart;
		public long filesize;
		public int compressedlen;
		public bool iscompressed
		{
			get { return compressedlen != filesize; }
		}

	}

	struct tModFile
	{
		public string path;
		public long size;
		public byte[] data;
	}

	struct tModInfo
	{
		public Version modloaderversion;
		public Version modversion;
		public string modname;
		public byte[] modhash;
		public byte[] modsignature;
		public int filecount;
	}

	interface ItMod
	{
		bool HasFile(string path);
		byte[] GetFile(string path);
		void WriteFile(string path, byte[] data);
		bool ReadMod();
		bool DumpFile(string outputpath, string filename);
		void DumpFiles(string outputpath);
		void DumpFiles(string outputpath, System.Action<string> func);
		void DumpFiles(string outputpath, System.Func<string, bool> func);
	}

	class tModBase : ItMod, IEnumerable<tModFile>, IEnumerable
	{
		protected tModInfo info;

		protected IDictionary<string, tModFileInfo> files = new Dictionary<string, tModFileInfo>();

		protected string     path;
		protected FileStream tempfile;
		protected string     tempfilepath = Path.GetTempFileName();

		public tModBase(string path)
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

		~tModBase()
		{
			if (this.tempfile != null)
				this.tempfile.Close();
		}

		virtual public bool HasFile(string path)
		{
			return this.files.ContainsKey(path.Replace("\\", "/"));
		}

		virtual public byte[] GetFile(string path)
		{
			throw new NotImplementedException();
		}

		virtual public void WriteFile(string path, byte[] data)
		{
			string dirpath = Path.GetDirectoryName(path);
			if (!Directory.Exists(dirpath))
			{
				Directory.CreateDirectory(dirpath);
			}
			File.WriteAllBytes(path, data);
		}

		virtual public bool ReadMod()
		{
			throw new NotImplementedException();
		}

		virtual public bool DumpFile(string outputpath, string filename)
		{
			throw new NotImplementedException();
		}

		virtual public void DumpFiles(string outputpath)
		{
			foreach (string fileName in this.files.Keys)
			{
				DumpFile(outputpath, fileName);
			}
		}

		virtual public void DumpFiles(string outputpath, System.Action<string> func)
		{
			foreach (string fileName in this.files.Keys)
			{
				func(fileName);
				DumpFile(outputpath, fileName);
			}
		}

		virtual public void DumpFiles(string outputpath, System.Func<string, bool> func)
		{
			foreach (string fileName in this.files.Keys)
			{
				if (!func(fileName))
					continue;
				DumpFile(outputpath, fileName);
			}
		}

		virtual public IEnumerator<tModFile> GetEnumerator()
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