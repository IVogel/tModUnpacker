using System;
using System.IO;

namespace tModUnpacker
{
	class main
	{
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
			tMod mod = new tMod(args[0]);
			mod.ReadMod();
			mod.DumpFiles(outputpath, filename => Console.WriteLine($"\tWriting: {filename}"));
		}
	}
}
