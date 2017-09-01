using System;
using System.IO;
using System.Xml.Linq;


namespace KladrService.Kladr
{
    public static class XElementExt
    {
        /// <summary>
        /// Метод раширения класса XDocument
        /// </summary>
        /// <param name="xDocument"> Ссылка на объект класса который расширяем </param>
        /// <param name="elementName"> Наименование элемента вложенного в корневой (Root) </param>
        /// <returns></returns>
        public static XElement GetElement(this XDocument xDocument, string elementName)
        {
            var xElement = xDocument.Element("Root");
            if (   xElement?.Element(elementName) != null )
            {
                return xElement.Element(elementName);
            }
            Log.Instance.WriteLine("Данные элемента XML:{0} не доступны!", elementName);
            return null;
        }
    }
    
    public static class Settings
    {
        public struct XmlData
            {
              public string CheckingPageUrl;
              public string SourceUrl;
              public int IntervalCheckPage;
              public string RegularTemplate;
              public string MailRecipients;
              public string ExeptionRecipient; 
              public string Host;
              public int Port;
              public string MailBox;
              public string Password;
              public DateTime StartTime;
            }

        public static int Interval = 15*60000; // Интервал проверок  минуты

      //  private static XMLData myData;
        public static XmlData MyXmlData;

        public static XmlData Load(string afilename)
        {
            
            if ( File.Exists(afilename) )
            {
               Log.Instance.WriteLine("Файл настроек найден."); 
               var xDoc = XDocument.Load(afilename, LoadOptions.None);

               MyXmlData.CheckingPageUrl = (xDoc.GetElement("ChPageUrl")!=null) ? (string) xDoc.GetElement("ChPageUrl") : "http://www.gnivc.ru/inf_provision/classifiers_reference/kladr/";
               MyXmlData.SourceUrl = (xDoc.GetElement("KLADR_Url") != null) ? (string) xDoc.GetElement("KLADR_Url"): "http://www.gnivc.ru/html/gnivcsoft/KLADR/";
               MyXmlData.RegularTemplate = (xDoc.GetElement("Reg_Temp") != null) ? (string) xDoc.GetElement("Reg_Temp") : @".*Дата актуальности - (\d{1,2}.\d{1,2}.\d{4}).*";
               MyXmlData.MailRecipients = (xDoc.GetElement("MailRecipients") != null) ? (string)xDoc.GetElement("MailRecipients"): "karuna@mail.ru";
                MyXmlData.Host = (string)xDoc.GetElement("SMTP").Attribute("host");
                if (! int.TryParse((string)xDoc.GetElement("SMTP").Attribute("port"), out MyXmlData.Port) ) MyXmlData.Port = 587;
                MyXmlData.MailBox = (string)xDoc.GetElement("SMTP").Attribute("box");
                MyXmlData.Password = (string)xDoc.GetElement("SMTP").Attribute("psw");
               
               if (!int.TryParse((string) xDoc.GetElement("Interval"), out MyXmlData.IntervalCheckPage))
                {
                    MyXmlData.IntervalCheckPage = 5 * 60000;
                }
                Log.Instance.WriteLine("Настройки загружены."); 
            }
           else
           {
               Log.Instance.WriteLine("Файл настроек не найден."); 
               MyXmlData.CheckingPageUrl = "http://www.gnivc.ru/inf_provision/classifiers_reference/kladr/";
               MyXmlData.SourceUrl = "http://www.gnivc.ru/html/gnivcsoft/KLADR/";
               MyXmlData.RegularTemplate = @".*Дата актуальности - (\d{1,2}.\d{1,2}.\d{4}).*";
               MyXmlData.IntervalCheckPage = 5 * 60000;
               MyXmlData.MailRecipients = "dmitry.karuna@gmail.ru";
                object[] attArray = {
                    new XAttribute("host", "smtp.gmail.com"),
                    new XAttribute("port", 587),
                    new XAttribute("box", "alerter.dnk@gmail.com"),
                    new XAttribute("psw", "Erkjytybt1") 
                };


                var xDoc = new XDocument(
                   new XComment("Настройки доступа к КЛАДР ФНС России"),
                   new XElement("Root",
                       new XElement("ChPageUrl", MyXmlData.CheckingPageUrl),
                       new XElement("KLADR_Url", MyXmlData.SourceUrl),
                       new XElement("Reg_Temp", MyXmlData.RegularTemplate),
                       new XElement("Interval", MyXmlData.IntervalCheckPage.ToString()),
                       new XElement("MailRecipients", MyXmlData.MailRecipients),
                       new XElement("SMTP", attArray)
                       ));
                    
               
                xDoc.Save(afilename);
                Log.Instance.WriteLine("Настройки установлены по умолчанию.");
            }

            MyXmlData.ExeptionRecipient = MyXmlData.MailRecipients.Split(Convert.ToChar(","))[0];
            MyXmlData.StartTime = DateTime.Now;

            Log.Instance.WriteLine("IntervalCheckPage = " + MyXmlData.IntervalCheckPage.ToString()+
                      " \n CheckingPageURL = "+ MyXmlData.CheckingPageUrl +
                      " \n KLADR_URL = " + MyXmlData.SourceUrl +
                      " \n Reg_Temp = " + MyXmlData.RegularTemplate +
                      " \n MailRecipients = " + MyXmlData.MailRecipients);

            return MyXmlData;
        }
     
    }
}
