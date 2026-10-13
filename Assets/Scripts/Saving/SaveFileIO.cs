using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Saving
{
    /// <summary>
    /// Shared file handling for the save files: plain JSON in the Editor (so it can be inspected),
    /// XOR-obfuscated in builds, and written to a temp file first so a crash mid-write can't
    /// corrupt the existing file.
    /// </summary>
    public static class SaveFileIO
    {
        public const string EncryptionKey = "FlamesOfFaith2025!";

        /// <summary>Reads a save file written by <see cref="WriteAtomically"/>, or by an older obfuscated build.</summary>
        public static bool TryReadJson(string path, out string json)
        {
            json = null;
            if (!File.Exists(path)) return false;

            string contents = File.ReadAllText(path);
            json = contents.TrimStart().StartsWith("{") ? contents : XOR(contents, EncryptionKey);
            return true;
        }

        /// <summary>Writes <paramref name="json"/> through a temp file. Throws on failure; callers log.</summary>
        public static void WriteAtomically(string path, string json)
        {
            string contents = Application.isEditor ? json : XOR(json, EncryptionKey);
            string tempPath = path + ".tmp";

            File.WriteAllText(tempPath, contents, Encoding.UTF8);
            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }

        public static string XOR(string input, string key)
        {
            var output = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                output.Append((char)(input[i] ^ key[i % key.Length]));
            }
            return output.ToString();
        }
    }
}
