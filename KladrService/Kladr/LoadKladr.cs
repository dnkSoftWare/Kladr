using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using KladrService.Loaders;
using Timer = System.Timers.Timer;

namespace KladrService.Kladr
{
    public class LoadKladr
    {
        private int _aTimerCnt;
        private static Timer _aTimer;
        private readonly GetKladr _getKladr;
        private readonly List<ILoader> _loaders;


        /// <summary>
        /// Проверяет, что GetKladr скачал новый кладр и стартует загрузку КЛАДР в потребителей (реализаторов интерфейса ILoader)
        /// </summary>
        public LoadKladr()
        {
            _getKladr = new GetKladr(); 
            if (_getKladr != null)
            {
                _loaders = new List<ILoader>();
                _aTimerCnt = 0;
                _aTimer = new Timer(Settings.MyXmlData.IntervalCheckPage > 0 ? Settings.MyXmlData.IntervalCheckPage : 600000)
                 { Enabled = false};// Пока отключем
                
                _aTimer.Elapsed += OnTimedEvent;
            }
            else
            {
                throw new Exception("Объект GetKLADR не создан!");
            }
        }

        /// <summary>
        /// Добавляет в коллекцию загрузчик
        /// </summary>
        /// <param name="loader"></param>
        public LoadKladr AddLoader(ILoader loader)
        {
            _loaders.Add(loader);
            Log.Instance.WriteLine("Add loader: {0}",loader.GetType().Name);
            return this;
        }

        public void Start(bool immediatly = false)
        {
            if (_aTimer.Enabled) return;
            if (immediatly)
                OnTimedEvent(this, null); // первая проверка сразу
            else
                _aTimer.Enabled = true;
        }
        public void Stop()
        {
            if (_aTimer.Enabled) 
            _aTimer.Enabled = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            var haveException = false;

            AppWorking(_aTimerCnt++);
            _aTimer.Enabled = false; // таймер отключаем
            try
            {
                if ( _getKladr.HaveNewKladr() ) // Проверка доступности нового КЛАДРА
                {
                    GetKladr.PrepareOutBoxFolder();
                    _loaders.ForEach( l => { l.Load(_getKladr.NewDate); }); // Запуск всех добавленных загрузчиков потребителей
                }
            }
            catch (Exception ex)
            {
                haveException = true;
                //throw new Exception(ex.Message);
                Log.Instance.WriteError(ex.Message + Environment.NewLine + "Таймер сервиса остановлен, устраните ошибку и перезапустите сервис(KladrService)!");

            }
            finally
            {
                if(! haveException)
                 _aTimer.Enabled = true; // таймер возобновляем
            }
        }

        private void AppWorking(int cnt)
        {
            if (cnt % 100 == 0)
            {

               // Mail.SendMail("", $"Application is working. {cnt.DurationSpan()} ", Settings.MyXmlData.ExeptionRecipient);
                Mail.SendMail("", $"Application is working. {DateTime.Now - Settings.MyXmlData.StartTime} ", Settings.MyXmlData.ExeptionRecipient);
            }
        }
    }

    public static class ExtInt
    {
        public static TimeSpan DurationSpan(this int cnt)
        {
            var seconds = cnt*Settings.MyXmlData.IntervalCheckPage/1000;
            //return Settings.MyXmlData.StartTime.AddSeconds(seconds) - Settings.MyXmlData.StartTime;
            return Settings.MyXmlData.StartTime.AddSeconds(seconds) - Settings.MyXmlData.StartTime;
        }
    }

}
