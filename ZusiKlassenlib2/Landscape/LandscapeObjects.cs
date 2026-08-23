using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ZusiKlassenLib2.Landscape
{
    public interface ILandscapeObject { }

    public sealed class LandscapeObject : ILandscapeObject
    {
        public string Filename { get; private set; }

        public LandscapeObject(string filename)
        {
            Filename = filename;
        }
    }

    public static class LandscapeObjects
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LandscapeObjects));

        [StructLayout(LayoutKind.Sequential)]
        private struct FileData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string RelativePath;
            public long Size;
            public DateTime LastWriteTime;
        }

        private static readonly List<string> _blacklist = new();

        //---------------------------------------------------------------------
        public static List<Landschaft> EnumerateLandscapes(string path, bool recursive, params string[] excludeFolders)
        {
            bool IsExcluded(string dir)
            {
                foreach (string s in excludeFolders)
                {
                    if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
                        return true;
                }

                return false;
            }

            List<Landschaft> result = new();

            foreach (string s in Directory.EnumerateFiles(path, "*.lod.ls3", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where((s, b) => !IsExcluded(s)))
            {
                try
                {
                    LandschaftsDatei ld = new(s);
                    ld.Parse();
                    result.Add(ld.Root);
                }
                catch (Exception ex)
                {
#if NET48
                    Log.ErrorFormat("Couldn't read landscape file: {0}{1} {2}", s.Substring(path.Length), Environment.NewLine, ex.ToString());
#else
                    Log.ErrorFormat("Couldn't read landscape file: {0}{1} {2}", s[path.Length..], Environment.NewLine, ex.ToString());
#endif
                }
            }

            return result;
        }

        //---------------------------------------------------------------------
        public static List<ILandscapeObject> EnumerateLandscapeObjects_(string path, bool recursive, params string[] excludeFolders)
        {
            bool IsExcluded(string dir)
            {
                foreach (string s in excludeFolders)
                {
                    if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
                        return true;
                }

                return false;
            }

            var result = from s in Directory.EnumerateFiles(path, "*.lod.ls3", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                         where !IsExcluded(s)
                         select new LandscapeObject(s);

            return result.ToList<ILandscapeObject>();
        }

        //---------------------------------------------------------------------
        public static string GetHash(string path, params string[] excludeFolders)
        {
            if (excludeFolders != null && excludeFolders.Length > 0)
            {
                _blacklist.AddRange(excludeFolders);
            }

#if NET48
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s.Substring(path.Length)));
#else
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s[path.Length..]));
#endif

            using MemoryStream ms = new(3072 * 1024);
            foreach (string file in files)
            {
                FileInfo fi = new(file);
                FileData fd = default;
#if NET48
                fd.RelativePath = file.Substring(path.Length).ToLower();
#else
                fd.RelativePath = file[path.Length..].ToLower();
#endif
                fd.Size = fi.Length;
                fd.LastWriteTime = fi.LastWriteTime;
                byte[] contentBytes = fd.GetBytes();
                ms.Write(contentBytes, 0, contentBytes.Length);
            }

            ms.Seek(0, SeekOrigin.Begin);
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(ms);
            return Convert.ToBase64String(hash);
        }

        //---------------------------------------------------------------------
        private static bool IsBlacklisted(string dir)
        {
            foreach (string s in _blacklist)
            {
                if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
                    return true;
            }

            return false;
        }
    }
}
