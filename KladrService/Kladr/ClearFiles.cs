using System;
using System.IO;

namespace KladrService.Kladr
{
    /// <summary>
    /// Класс для очистки каталога от старых файлов КЛАДР-а
    /// </summary>
    static class ClearFiles
    {
        /// <summary>
        /// Очистка каталога
        /// </summary>
        /// <param name="dir"> Каталог </param>
        /// <param name="filemask"> Удалять только файлы по маске </param>
        /// <param name="exeptfilescount"> исключить указанное кол-во файлов </param>
        public static void clear(string dir, string filemask, int exeptfilescount)
        {
            string adir = Directory.GetCurrentDirectory() + @"\" +dir;
            string[] fileslist = Directory.GetFiles(adir, filemask, SearchOption.TopDirectoryOnly);

            if (fileslist.GetLength(0) >= 2)
            {
                Array.Sort(fileslist);

                for (int j = fileslist.GetLowerBound(0); j <= fileslist.GetUpperBound(0) - exeptfilescount; j++)
                {
                    try
                    {
                        File.Delete(fileslist[j]);
                        Log.Instance.WriteLine("File {0} deleted!", fileslist[j]);

                    }
                    catch (Exception e)
                    {
                        Log.Instance.WriteLine("File {0} not deleted! {1}", fileslist[j], e.Message);
                    }
                }

            }
        }
    }
}
