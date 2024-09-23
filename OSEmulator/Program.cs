using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace OSEmulator
{
    internal class OS
    {
        public static string KeyIdentifier = "--";

        public class VirtualPath
        {
            // Не заканчивается на "/"
            private static string _home = "VirtualOS";
            private string _zipPath;
            private string _currentPath;
            private ZipArchive _archive;

            public string CurrentPath => _currentPath.TrimEnd('/');

            public VirtualPath(string osDir)
            {
                _zipPath = osDir;
                _archive = ZipFile.OpenRead(_zipPath);
                _home = _archive.Entries[0].FullName.TrimEnd('/');
                _currentPath = _home;

                _archive.Dispose();
            }

            public IEnumerable<string> GetDirectoryFiles()
            {
                _archive = ZipFile.OpenRead(_zipPath);
                int parts = _currentPath.Split('/').Length;
                foreach(var file in _archive.Entries)
                {
                    if(file.FullName.StartsWith(_currentPath)
                        && file.FullName.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1 == parts)
                        yield return file.FullName.TrimEnd('/');
                }
            }

            private void Walkthrough(string rest)
            {
                if (rest.Length == 0)
                    return;
                //From root
                if (rest[0] == '/')
                {
                    _currentPath = _home;
                    Walkthrough(rest.Substring(1));
                    return;
                }
                //Up dir
                if (rest.StartsWith(".."))
                {
                    if (_currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length < 2)
                        return;
                    _currentPath = _currentPath.Substring(0, _currentPath.LastIndexOf('/'));
                    //Если ../
                    if(rest.Length > 2)
                        Walkthrough(rest.Substring(1));
                    return;
                }
                //From current dir
                if (rest.StartsWith("./"))
                {
                    Walkthrough(rest.Substring(2));
                    return;
                }

                //Cleared path
                string[] parts;
                if (!rest.Contains('/'))
                {
                    parts = [rest, ""];
                }
                else parts = rest.Split('/', 2);
                string forward = _currentPath + "/" + parts[0] ;
                if (_archive.GetEntry(forward + "/") == null)
                    throw new FileNotFoundException("Directory doesn't exist! -> " + forward);
                _currentPath = forward;
                Walkthrough(parts[1]);
            }

            public void SetPosition(string path)
            {
                _archive = ZipFile.OpenRead(_zipPath);
                string copy = _currentPath;
                try
                {
                    if(path == "~")
                    {
                        path = _home;
                        return;
                    }
                    Walkthrough(path);
                }
                catch (FileNotFoundException ex)
                {
                    _currentPath = copy;
                    Console.WriteLine(ex.Message);
                }
                _archive.Dispose();
            }

            public void CloseArchive()
            {
                _archive.Dispose();
            }
        }

        public class Logger
        {
            public string LogPath { get; private set; }
            private string _userName { get; set; }
            public List<String> Actions { get; set; }
            public Logger()
            {
                
            }

            public Logger(string path, string userName)
            {
                LogPath = path; 
                _userName = userName;
                Actions = new List<string>();
            }

            public void Log(string cmd)
            {
                Actions.Add($"[{DateTime.Now}] {_userName}: {cmd}");
            }

            public void WriteLogs()
            {
                using(StreamWriter writer = new StreamWriter(LogPath, false))
                {
                    string ser = JsonSerializer.Serialize(this);
                    writer.WriteLine(ser);
                }
            }
        }

        public enum Keys
        {
            user,
            pcn,
            vrtlos,
            logpth,
            startsrc
        };

        public enum Commands
        {
            exit,
            ls,
            cd,
            whoami,
            undefined
        }

        private string _pc;
        private string _userName;
        private string _startScriptPath;
        private VirtualPath _virtualPath;
        private Logger _logger;

        private string _cliStr => _userName + "@" + _pc + ":" + _virtualPath.CurrentPath + "$ ";

        private OS(string pc, string user, string virt, string log, string src)
        {
            if (!IsKeyCorrect(user))
                Stop("Wrong user name! -> " + user);
            if (!IsKeyCorrect(pc))
                Stop("Wrong pc name! -> " + pc);
            if (!IsKeyCorrect(virt) || !Path.Exists(virt))
                Stop("Wrong virtualOS path! -> " + virt);

            _pc = pc;
            _userName = user;
            _virtualPath = new VirtualPath(virt);
            if (IsKeyCorrect(log))
                _logger = new Logger(log, _userName);
            if (IsKeyCorrect(src))
            {
                using(StreamReader reader = new StreamReader(src))
                {
                    List<string> list = [];
                    while (!reader.EndOfStream)
                    {
                        list.Add(reader.ReadLine());
                    }
                    ExecuteLines(list);
                }
            }
        }
        private bool IsKeyCorrect(string key)
        {
            return !(String.IsNullOrWhiteSpace(key) || String.IsNullOrEmpty(key));
        }

        public static OS Init(string[] args)
        {
            Console.Clear();
            ConsoleColor userColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            if (args.Length == 0)
            {
                Console.WriteLine("Ошибка! Не заданы ключи запуска!");
                Console.ResetColor();
                Environment.Exit(-1);
            }
            if (args.Length % 2 != 0)
            {
                Console.WriteLine("Ошибка! Неверно заданы ключи запуска!" +
                    " Их должно быть чётное количество, но найдено: " + args.Length);
                Console.ResetColor();
                Environment.Exit(-1);
            }

            InjectKeys(args, out string user, out string pc, out string virt, out string log, out string src);

            return new OS(pc, user, virt, log, src);
        }

        private static void InjectKeys(string[] args, out string user,
            out string pc, out string virt, out string log, out string src)
        {
            user = String.Empty;
            pc = String.Empty;
            virt = String.Empty;
            log = String.Empty;
            src = String.Empty;

            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i].Equals(KeyIdentifier + Keys.user.ToString()) && i + 1 < args.Length)
                {
                    user = args[i + 1];
                }
                if (args[i].Equals(KeyIdentifier + Keys.pcn.ToString()) && i + 1 < args.Length)
                {
                    pc = args[i + 1];
                }
                if (args[i].Equals(KeyIdentifier + Keys.vrtlos.ToString()) && i + 1 < args.Length)
                {
                    virt = args[i + 1];
                }
                if (args[i].Equals(KeyIdentifier + Keys.logpth.ToString()) && i + 1 < args.Length)
                {
                    log = args[i + 1];
                }
                if (args[i].Equals(KeyIdentifier + Keys.startsrc.ToString()) && i + 1 < args.Length)
                {
                    src = args[i + 1];
                }
            }
        }

        public void Stop(string message = "")
        {
            _logger?.WriteLogs();
            Console.WriteLine(message);
            Console.ResetColor();
            Environment.Exit(0);
        }

        private void PrintWelcome()
        {
            Console.WriteLine($"Hello {_userName}!");
            Console.WriteLine();
        }

        public void Start()
        {
            PrintWelcome();
            ReadLine();
        }

        private Commands TranslateCommand(string command)
        {
            Commands cmd = Commands.undefined;

            foreach (var el in Enum.GetValues(typeof(Commands)))
            {
                if (command.Equals(el.ToString()))
                {
                    cmd = (Commands)el;
                    break;
                }
            }
            return cmd;
        }

        private void ReadLine()
        {
            Console.Write(_cliStr);
            string input = Console.ReadLine();
            ExecuteLine(input);
            ReadLine();
        }

        public void ExecuteLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                ExecuteLine(line);
            }
        }

        public void ExecuteLine(string input)
        {
            string[] commandParts = input.Split();
            Commands cmd = TranslateCommand(commandParts[0]);
            _logger?.Log(input);
            if (cmd == Commands.whoami)
            {
                Console.WriteLine(_userName);
            }
            else if (cmd == Commands.ls)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var el in _virtualPath.GetDirectoryFiles())
                    Console.WriteLine(el);
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (cmd == Commands.cd)
            {
                if (commandParts.Length == 2)
                    _virtualPath.SetPosition(commandParts[1]);
            }
            else if (cmd == Commands.exit)
            {
                Stop();
            }
            else
            {
                Console.WriteLine("Invalid command!");
            }
        }
    }

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
            args = ["--user",
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

            //dotnet run --project C:\Users\Policarp\source\repos\OSEmulator\OSEmulator --user policarp --pcn DESKTOP --vrtlos C:\\Users\\Policarp\\Desktop\\VirtualOS.zip --logpth C:\\Users\\Policarp\\Desktop\\log.txt --startsrc C:\\Users\\Policarp\\Desktop\\script.txt


            OS system = OS.Init(args);
            system.Start();
            //C:\Users\Policarp\source\repos\OSEmulator\OSEmulator.sln
            //OS.VirtualPath virtualPath = new OS.VirtualPath("C:\\Users\\Policarp\\Desktop\\VirtualOS.zip");
            //OS.VirtualPath virtualPath = new OS.VirtualPath("C:\\Users\\Policarp\\source\\repos\\OSEmulator");
            //while (true)
            //{
            //    virtualPath.SetPosition(Console.ReadLine());
            //    Console.WriteLine(virtualPath.CurrentPath);

            //}
        }

    }
}
