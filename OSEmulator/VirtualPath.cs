using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSEmulator
{
    //Output path ignores last /
    //Path begins with home like VirtualOS/bin
    //Never add by yourself / at the end
    public class VirtualPath
    {
        public readonly string HomePath = "VirtualOS";
        private static string _default = "";
        private string _zipPath;
        private string _currentPath;
        private ArchiveAccess _archive;

        private string _admin;

        private Dictionary<string, string> _owners;

        public string CurrentPath => _currentPath.TrimEnd('/');

        public VirtualPath(string osDir, string admin)
        {
            _archive = new ArchiveAccess(osDir);
            HomePath = _archive.GetEntryName(_archive.GetEntries().First());
            _currentPath = _default;
            _owners = new Dictionary<string, string>();
            _admin = admin;
        }

        public IEnumerable<Tuple<string, string>> GetDirectoryFiles()
        {
            int parts = _currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
            foreach (var file in _archive.GetArchivesFileNames())
            {
                var splitted = file.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length - 1 == parts && file.StartsWith(_currentPath))
                {
                    yield return new Tuple<string, string>(splitted[splitted.Length - 1],
                       file);
                }
            }
        }

        

        private void Walkthrough(string rest)
        {
            if (rest.Length == 0)
                return;
            //From root
            if (rest[0] == '/')
            {
                _currentPath = _default;
                Walkthrough(rest.Substring(1));
                return;
            }
            //Up dir
            if (rest.StartsWith(".."))
            {
                if(_currentPath.Contains('/'))
                    _currentPath = _currentPath.Substring(0, _currentPath.LastIndexOf('/'));
                //Если ../
                if (rest.Length > 2)
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
            string forward = _currentPath + "/" + parts[0];
            if (!_archive.IsEntryExist(forward))
                throw new FileNotFoundException("File doesn't exist! -> " + forward);
            _currentPath = forward;
            Walkthrough(parts[1]);
        }

        public void ChangeOwner(string filePath, string owner)
        {
            try
            {
                string path = GetAbsolutePosition(filePath);
                if (string.IsNullOrEmpty(path))
                    return;
                if (_owners.ContainsKey(path))
                    _owners[path] = owner;
                else _owners.Add(path, owner);
            }
            catch(FileNotFoundException ex){
                Console.WriteLine(ex.Message);
            }
            
        }

        public string GetOwner(string filePath)
        {
            try
            {
                //Check path
                GetAbsolutePosition(filePath);
                if (string.IsNullOrEmpty(filePath)) return "Invalid file path";
                return (_owners.ContainsKey(filePath) ? _owners[filePath] : _admin);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return "Fail";
            }
        }

        public string GetAbsolutePosition(string path)
        {
            string copy = _currentPath;
            string res = _default;
            try
            {
                if (path == "~")
                {
                    _currentPath = _default;
                }
                Walkthrough(path);
                res = _currentPath;
            }
            catch (FileNotFoundException)
            {
                _currentPath = copy;
                throw;
            }
            _currentPath = copy;
            return res;
        }

        public void SetPosition(string path)
        {
            try
            {
                _currentPath = GetAbsolutePosition(path);
            }
            catch(FileNotFoundException ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
