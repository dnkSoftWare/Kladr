using System.Diagnostics;
using System.IO;
using KladrService.Kladr;
using System.Threading;

namespace KladrService.Loaders
{
    /// <summary>
    /// Интерфейс потребителей КЛАДРа
    /// </summary>
    public interface ILoader
    {
        void Load(string newdate);
        void LoaderRun();
    }

    internal interface ILoadTest
    {
        void LoaderRun();
    }

    public class Loader : ILoadTest
    {
        public void LoaderRun()
        {
            Log.Instance.WriteLine(" {0} Loader run!", GetType());
        }

        public void AddTo(LoadKladr lk)
        {
            lk.AddLoader((ILoader) this);
        }
    }

    public class ArmFssLoader : Loader, ILoader
    {
        protected bool ProcessLoaded(string nameProcc)
        {
            return (Process.GetProcessesByName(nameProcc).Length != 0);
        }

        public void Load(string newdate) // перегружаем под себя
        {
            var loadKladrExe = "load_kladr.exe";
            var loadKladr_wo = "load_kladr";
            LoaderRun();
            if (File.Exists(loadKladrExe))
            {

                try
                {
                    foreach (System.Diagnostics.Process myProc in Process.GetProcessesByName(loadKladr_wo))
                    {
                      myProc.Kill();
                        Thread.Sleep(5000);
                    }

                    var mr = Settings.MyXmlData.MailRecipients.Replace(",", ";");
                    Log.Instance.WriteLine("Запуск процесса загрузки КЛАДР для arm_fss...");
                    MyProcess.CreateProc(loadKladrExe, "-d:" + newdate + " -a -mr:" + mr);
                    // автостарт процесса загрузки без ожидания окончания его работы

                }
                catch (System.Exception ex)
                {
                    Log.Instance.WriteError("Процесс {0} уже запущен! {1}", loadKladrExe,ex.Message);
                }
            }
            else
            {
                Log.Instance.WriteError("Error: {0} not found !", loadKladrExe);
            }
        }
    }

    public class WozmLoader : Loader, ILoader
    {
        public void Load(string newdate)
        {
            LoaderRun();
            Log.Instance.WriteLine("Запуск копирования КЛАДР для ВОЗМ...\n");
            MyProcess.ExecuteCommandSync(@"move /Y base" + newdate + @".7z outbox");
        }
    }
}