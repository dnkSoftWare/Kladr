using System;
using System.IO;
using System.Net;
using System.Threading;


namespace KladrService.Kladr
{
    public class GetKladr
    {
        public string NewDate;
        public static string Outboxdir = "OUTBOX";
        
        /// <summary>
        /// Проверка актуальной даты на странице налоговой
        /// </summary>
        private readonly CheckHtml _checkHtml;

        /// <summary>
        ///  Получение архива КЛАДР с сайта налоговой и распаковка его в папку temp
        /// </summary>

        public GetKladr()
        {
            _checkHtml = new CheckHtml();

            if (_checkHtml == null)
            {
                throw new Exception("Нет объекта CheckHtml проверки странички!");
            }
        }

        public bool HaveNewKladr()
        {
            string pageDate;
            if (_checkHtml.Check(out pageDate)) // Проверили страничку и можем забирать новый КЛАДР
            {
                var actualFileName = "base" + pageDate + ".7z";
                try
                {
                    const string baseZ = "Base.7z";
                    if (SaveKladrFile(baseZ, actualFileName)) // Если удалось нормально скачать сохраняем
                    {

                        Thread.Sleep(2000); // задержка чтоб файл отпустился...

                        string outStr;
                        if (_7Za.Unzip(actualFileName,"temp", out outStr))
                            // распаковка полученного архива в папку temp
                        {
                            Log.Instance.WriteLine("Archive {0} unpacked!", actualFileName);

                            _checkHtml.UpdateActualDate(); // то можно обновить дату актуальности
                            NewDate = pageDate;
                            Log.Instance.WriteLine("Дата актуальности обновлена!");
                            return true;
                        }
                        else
                        {
                            Log.Instance.WriteLine("Archive {0} not unpacked!", actualFileName);
                            Log.Instance.WriteLine(outStr);
                            throw new Exception("Archive "+actualFileName+" not unpacked!");
                        }
                    }
                    else
                    {
                        Log.Instance.WriteLine("File {0} not downloaded!", baseZ);
                        throw new Exception("File "+ baseZ + " not downloaded!");
                    }
  

                }
                catch (Exception e)
                {
                  throw new Exception(e.Message); 
                }
                
            }
            else
            {
                return false;
            }
            
        }
        /// <summary>
        /// Подготовка папки OUTBOX
        /// Удаление всех файлов кроме последних
        /// </summary>
        public static void PrepareOutBoxFolder()
        {
            if (! Directory.Exists(Outboxdir))
            {
                Directory.CreateDirectory(Outboxdir);
            }
            ClearFiles.clear(Outboxdir, "ADDRESS*.zip", 1); // Подчищаем все кроме самого последнего
            ClearFiles.clear(Outboxdir, "base*.7z", 1);
        }

        /// <summary>
        /// Скачивание и сохранение архивного файла с КЛАДРОМ
        /// </summary>
        /// <param name="downloadfilename">Название удалённого файла Base.7z </param>
        /// <param name="saveFileName">Название локального файла для сохранения</param>
        /// <returns></returns>
        private bool SaveKladrFile(string downloadfilename, string saveFileName)
        {
            try
            {
                Log.Instance.WriteLine("Start download!");
                File.WriteAllBytes(saveFileName, GetFileViaHttp(Settings.MyXmlData.SourceUrl + downloadfilename));
                Log.Instance.WriteLine("File downloaded!");
                return true;
            }
            catch (Exception e)
            {
                Log.Instance.WriteError("File not downloaded: "+e.Message );
                //                throw;
                return false;
            }
            
        }

        /// <summary>
        /// Получение удаленного файла по HTTP
        /// </summary>
        /// <param name="url">полный путь(URL) к скачиваемому файлу</param>
        /// <returns> массив байт </returns>
        public byte[] GetFileViaHttp(string url)
        {
            using (var client = new WebClient())
            {
               // return client.DownloadDataAsync(new Uri(url));
               return client.DownloadData(url);
            }
        }
    }
}
