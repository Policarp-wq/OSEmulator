using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace OSEmulator
{
   

    internal class Program
    {
        static void Main(string[] args)
        {
            //dotnet run --project C:\Users\Policarp\source\repos\OSEmulator\OSEmulator
            //user,
            //pcn,
            //vrtlOS,
            //logpth,
            //startsrc
            string[] test = ["--user",
                "Policarp",
                "--pcn",
                "MHHUT-DDOS",
                "--vrtlos",
                "C:\\Users\\Policarp\\Desktop\\VirtualOS.zip",
                "--logpth",
                "C:\\Users\\Policarp\\Desktop\\log.txt",
                "--startsrc",
                "C:\\Users\\Policarp\\Desktop\\script.txt"
            ];

            //dotnet run --project C:\Users\Policarp\source\repos\OSEmulator\OSEmulator
            //--user policarp --pcn DESKTOP --vrtlos C:\\Users\\Policarp\\Desktop\\VirtualOS.zip --logpth C:\\Users\\Policarp\\Desktop\\log.txt --startsrc C:\\Users\\Policarp\\Desktop\\script.txt

            OS system = OS.Init(args);
            system.Start();
        }

    }
}
