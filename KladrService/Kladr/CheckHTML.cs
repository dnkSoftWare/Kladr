using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KladrService.Kladr
{
    /// <summary>
    /// Проверка HTML странички на изменения даты актуальности
    /// </summary>
    class CheckHtml
    {
        /// <summary>
        /// Файл для сохранения даты последнего обновления КЛАДРА
        /// </summary>
        private static string _actualFileName = @"ActualDate.xml";
        /// <summary>
        /// Свойство сигнализирующее о наличии новой даты
        /// </summary>
        /// 
        public bool IsNew {
            get { return _isNew; } 
            set { _isNew = value; if (_isNew)  SendMailAlert(_onPageDate);
            } }

        private DateTime _actualDate;

        /// <summary>
        /// Свойство содержит актуальную дату на данный момент
        /// </summary>
        public DateTime ActualDate {
            get
            {
                if (File.Exists(_actualFileName) ) // && (_actualDate == new DateTime(2015, 01, 01)))
                {
                    var xDoc = XDocument.Load(_actualFileName, LoadOptions.None);
                    _actualDate = (DateTime) xDoc.Element("ActualDate");
                    Log.Instance.WriteLine("Cохранённая дата актуальности "+_actualDate.ToString("yyyy-MM-dd"));
                }
                if (_actualDate < _onPageDate)
                {
                    Log.Instance.WriteLine("Получена новая дата со страницы УФНС: " + _onPageDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    Log.Instance.WriteLine("Дата со страницы : " + _onPageDate.ToString("yyyy-MM-dd"));
                }
                    
                return _actualDate;
            }
            set
            {
                if (value > _actualDate)
                {
                    var xDoc = new XDocument(
                         new XElement("ActualDate", value)
                         );
                    xDoc.Save(_actualFileName);  
                }
                _actualDate = value;
            } 
        }

        /// <summary>
        /// Дата на проверяемой страничке
        /// </summary>
        private DateTime _onPageDate;

        private bool _isNew;

        private static void SendMailAlert(DateTime value)
        {
             Mail.SendMail("Новый КЛАДР!",
                "Обнаружен новый архив с КЛАДР : " + value.ToString("yyyy-MM-dd") +
                Environment.NewLine + "Через некоторое время необходимо проверить доступность нового KLADR по этой ссылке.\n" +
                Environment.NewLine + "http://docs-test.fss.ru/KLADRMonitor/Home/LastFileInfoStr" +
                Environment.NewLine + "Ожидается дополнительное уведомление по почте о готовности адресной базы FDB.");
            Log.Instance.WriteLine("Email alert sended! ");
        }

        /// <summary>
        ///  Обновляем актуальную дату и пишем в файл
        /// </summary>
        public void UpdateActualDate()
        {
            ActualDate = _onPageDate;
        }
        /// <summary>
        /// Умеет забирать HTML страничку в виде строки и сравнивать дату со странички с актуальной которая берется из файла или по дефолту
        /// </summary>
        public CheckHtml()
        {
            _actualDate = new DateTime(2015, 01, 01); // aActualDate;
            _onPageDate = new DateTime(2015, 01, 01);
            _isNew = false;
        }

        public bool Check(out string pageDate)
        {
            _onPageDate = GetDate(); // Смотрим дату на страничке
            IsNew = (ActualDate < _onPageDate); // Устанвливаем флаг, ели новая дата больше актуальной.
            pageDate = _onPageDate.ToString("yyyy-MM-dd");
            return IsNew;
        }

        private string GetTextFrom(string pattern)
        {
            var s = "";

            using (var client = new WebClient())
            {
                try
                {
                    s = client.DownloadString(Settings.MyXmlData.CheckingPageUrl);
                }
                catch (WebException webExceptionex)
                {
                    Log.Instance.WriteException(webExceptionex);
                }

                var myBytes = Encoding.GetEncoding(1251).GetBytes(s);

                s = Encoding.UTF8.GetString(myBytes);

                if (Regex.IsMatch(s, pattern))
                {
                    var m = Regex.Match(s, pattern);
                    s = m.Groups[1].Value;
                }
                else
                {
                    Log.Instance.WriteLine("Строка не найдена! "+pattern);
                    s = ActualDate.ToString("yyyy-MM-dd");
                }
                
            }
            return s;
        }

        private DateTime GetDate()
        {
            return DateTime.Parse(GetTextFrom(Settings.MyXmlData.RegularTemplate) );
        }

    }
}
