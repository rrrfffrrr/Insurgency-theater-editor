using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Insurgency_theater_editor
{
    public class ProjectFolder
    {
        public static readonly string[] GameMode = { "ambush", "checkpoint", "conquer", "elimination", "firefight", "hunt", "occupy", "push", "skirmish", "strike", "survival" };

        private string folderPath;
        public string FolderPath {
            get {
                return folderPath;
            }
            private set {
                if (Directory.Exists(value)) {
                    folderPath = value;
                    ReloadFiles();
                }
            }
        }

        /// <summary>
        /// theater, files
        /// game mode theater will be contain in this collection too
        /// </summary>
        private Dictionary<string, List<string>> theaters;
        public Dictionary<string, List<string>>.KeyCollection Theaters {
            get {
                return theaters.Keys;
            }
        }
        public string[] GetFileListFromTheater(string theater) {
            return theaters[theater].ToArray();
        }

        /// <summary>
        /// file, theater
        /// theater will be null if it's not game mode theater
        /// </summary>
        private Dictionary<string, string> files;
        private List<string> cachedFiles;
        public int Count {
            get {
                if (files == null)
                    return 0;
                return files.Count;
            }
        }
        public string GetFileName(int index)
        {
            if (files == null || index < 0 || files.Count <= index)
            {
                return string.Empty;
            }

            return cachedFiles[index];
        }
        public string[] GetFileList()
        {
            return cachedFiles.ToArray();
        }

        public ProjectFolder(string path)
        {
            this.FolderPath = path;
        }

        public void ReloadFiles()
        {
            if (files == null)
            {
                files = new Dictionary<string, string>();
            }
            if (theaters == null)
            {
                theaters = new Dictionary<string, List<string>>();
            }
            files.Clear();
            theaters.Clear();

            string[] gameModePostfixs = GameMode.Clone() as string[];
            for (int i = 0; i < gameModePostfixs.Length; ++i)
            {
                gameModePostfixs[i] = "_" + gameModePostfixs[i];
            }

            string[] fileNames = Directory.GetFiles(folderPath, "*.theater");
            // 1. find theaters with game mode
            foreach (string path in fileNames)
            {
                string justName = Path.GetFileNameWithoutExtension(path);
                foreach (string postfix in gameModePostfixs)
                {
                    if (justName.EndsWith(postfix))
                    {
                        string key = justName.Substring(0, justName.Length - postfix.Length);
                        if (!theaters.ContainsKey(key))
                            theaters.Add(key, new List<string>());
                        break;
                    }
                }
            }

            // 2. add files, if start with theater, add it to theater list
            foreach (string path in fileNames)
            {
                string name = Path.GetFileName(path);
                string theater = null;

                foreach (string prefix in theaters.Keys) {
                    if (name.StartsWith(prefix))
                    {
                        theater = prefix;
                        break;
                    }
                }

                files.Add(name, theater);
                if (theater != null)
                {
                    theaters[theater].Add(name);
                }
            }

            // 3. update cachedFiles with files
            cachedFiles = new List<string>(files.Keys);
        }
    }
}
