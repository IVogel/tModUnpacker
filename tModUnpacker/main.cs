using System;
using System.IO;
using System.Text;

namespace tModUnpacker
{
	class main
	{
		static ItMod get_suitable_unpacker(string path)
		{
			Version ver;
			using (FileStream fileStream = File.OpenRead(path))
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != "TMOD")
					throw new IOException("Can't read tMod magic bytes");
				ver = new Version(binaryReader.ReadString());
			}

			if (ver < new Version(0, 11))
				return new tMod_Old(path);
			else
				return new tMod(path);
		}

		static void Main(string[] args)
		{
			int  nArgs = args.Length;
			if (nArgs < 1)
			{
				string help = "Usage:\n" +
				$"\t{AppDomain.CurrentDomain.FriendlyName} \"/path/to/tmod/file.tmod\"\n" +
				"\tWill extract mod in the same folder where .tmod file located is.\n" +
				"\n" +
				$"\t{AppDomain.CurrentDomain.FriendlyName} \"/path/to/tmod/file.tmod\" \"/path/to/outputfolder\"\n" +
				"\tWill extract in specified folder.\n";
				Console.Write(help);
				return;
			}
			string outputpath;
			if (nArgs < 2)
			{
				outputpath = Path.GetDirectoryName(args[0]);
			}
			else
			{
				outputpath = args[1];
			}
			ItMod mod = get_suitable_unpacker(args[0]);
			mod.ReadMod();
			mod.DumpFiles(outputpath, filename => Console.WriteLine($"\tWriting: {filename}"));
		}
	}
}
