using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OSEmulator
{
    public class OS
    {
        public static string KeyIdentifier = "--";
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
                using (StreamWriter writer = new StreamWriter(LogPath, false))
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
            chown,
            help,
            undefined
        }

        private string _pc;
        private string _userName;
        private string _startScriptPath;
        private VirtualPath _virtualPath;
        private Logger _logger;

        public string UserName => _userName;

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
            _virtualPath = new VirtualPath(virt, _userName);
            if (IsKeyCorrect(log))
                _logger = new Logger(log, _userName);
            if (IsKeyCorrect(src))
            {
                using (StreamReader reader = new StreamReader(src))
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
            Console.ForegroundColor = ConsoleColor.Green;
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid startup keys!");
                Console.ResetColor();
                Environment.Exit(-1);
            }
            if (args.Length % 2 != 0)
            {
                Console.WriteLine("Invalid startup keys!" +
                    "There are must be even number of keys, but found: " + args.Length);
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
                {
                    string printMsg = el.Item1;
                    if (commandParts.Length == 2 && commandParts[1].Equals("-l"))
                    {
                        printMsg += "-->" + _virtualPath.GetOwner(el.Item2);
                    }
                    Console.WriteLine(printMsg);
                }
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (cmd == Commands.cd)
            {
                if (commandParts.Length == 2)
                    _virtualPath.SetPosition(commandParts[1]);
            }
            else if (cmd == Commands.chown)
            {
                if (commandParts.Length == 3)
                    _virtualPath.ChangeOwner(commandParts[1], commandParts[2]);
            }
            else if (cmd == Commands.help)
            {
                foreach (var el in Enum.GetValues(typeof(Commands)))
                {
                    if ((Commands)el != Commands.help && (Commands)el != Commands.undefined)
                    {
                        Console.WriteLine(el.ToString());
                    }
                }
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

        private void Print(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
