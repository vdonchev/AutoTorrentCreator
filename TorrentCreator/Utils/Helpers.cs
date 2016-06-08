namespace TorrentCreator.Utils
{
    using System.IO;
    using System.Linq;

    public static class Helper
    {
        public static string GetFolderName(string path)
        {
            var dirName = new DirectoryInfo(path).Name;

            return dirName;
        }

        public static string ToAlphaNumberc(string text)
        {
            var res = new string(
                text
                    .Where(ch => char.IsLetterOrDigit(ch) ||
                                 char.IsWhiteSpace(ch) ||
                                 ch == '_' ||
                                 ch == '-').ToArray());

            res = res.Replace(' ', '_').ToLower();

            return res;
        }
    }
}