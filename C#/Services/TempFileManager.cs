using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public class TempFileManager
    {
        public static string TempFileDirectory { get; set; }

        static TempFileManager()
        {
            TempFileDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "TempDirectory");
        }

        public static void Clear()
        {
            if (Directory.Exists(TempFileDirectory))
            {
                Directory.Delete(TempFileDirectory, true);
            }
        }

        public static void Delete(string fileName)
        {
            string fullFile = Path.Combine(TempFileDirectory, fileName);
            if (File.Exists(fullFile))
            {
                File.Delete(fullFile);
            }
        }
    }
}
