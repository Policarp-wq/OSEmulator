using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSEmulator
{
    public class ArchiveAccess
    {
        public readonly string ArchivePath;
        //VirualOS/
        private string _homeDir;
        public ArchiveAccess(string archivePath)
        {
            ArchivePath = archivePath;
            _homeDir = GetEntries().First().FullName.TrimEnd('/');
        }
        public ReadOnlyCollection<ZipArchiveEntry> GetEntries()
        {
            using (ZipArchive archive = ZipFile.OpenRead(ArchivePath))
            {
                return archive.Entries;
            }
        }

        public string GetEntryName(ZipArchiveEntry entry)
        {
            if (entry == null)
                return string.Empty;
            return GetTrimmedPath(entry.FullName);
        }

        public IEnumerable<string> GetArchivesFileNames()
        {
            foreach(var entry in GetEntries())
            {
                yield return GetEntryName(entry);
            }
        }
        //Removing homedir
        private string GetTrimmedPath(string path)
        {
            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < parts.Length; i++)
            {
                sb.Append("/" + parts[i]);
            }
            return sb.ToString();
        }

        public ZipArchiveEntry GetEntryByPath(string path)
        {
            using(ZipArchive archive = ZipFile.OpenRead(ArchivePath))
            {
                return archive.GetEntry(_homeDir + path + (path.Contains('.') ? "" : "/"));
            }
        }

        public bool IsEntryExist(string path)
        {
            return !GetEntryName(GetEntryByPath(path)).Equals(string.Empty);
        }

    }
}
