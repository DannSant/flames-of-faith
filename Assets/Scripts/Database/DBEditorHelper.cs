using UnityEngine;
using System.IO;

namespace Game.Database {
    public static class DBEditorHelper
    {
        // Folder where DB is stored inside the Unity project
        public static string EditorDBFolder
        {
            get
            {
                // Normalize and go outside Assets folder
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string dbFolder = Path.Combine(projectRoot, "Database");

                // Ensure folder exists
                if (!Directory.Exists(dbFolder))
                    Directory.CreateDirectory(dbFolder);

                return dbFolder;
            }
        }

        // Full path to the SQLite DB file
        public static string DbPath => Path.Combine(EditorDBFolder, "effects.db");

        /// <summary>
        /// Ensures the database folder exists and returns the DB path.
        /// </summary>
        public static string EnsureDatabasePath()
        {
            if (!Directory.Exists(EditorDBFolder))
                Directory.CreateDirectory(EditorDBFolder);

            return DbPath;
        }
    }
}

