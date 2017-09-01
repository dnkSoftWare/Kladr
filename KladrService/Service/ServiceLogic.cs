using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using KladrService.Kladr;
using KladrService.Loaders;

namespace KladrService
{
    public partial class KladrService
    {
        public static string AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private LoadKladr Lk;

        private void InitWork()
        {
            Settings.Load("Settings.xml"); // загрузка настроек
            Lk = new LoadKladr(); // Основной объект LoadKladr
            Lk.AddLoader(new ArmFssLoader()) // для перекидывания КЛАДРа в свои подсистемы
                .AddLoader(new WozmLoader());
        }
        private void DoWork()
        {
            if (isRunning)
            {

                Lk.Start(true);
            }
            else
            {
                Lk.Stop();
            }
        }
    }
}
