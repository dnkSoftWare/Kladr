using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetAndLoad_KLADR
{
    class GetData
    {
        /// <summary>
        /// Папка где лежат DBF файлы скачанного и распакованного архива
        /// </summary>
        String DbfFolder;
        /// <summary>
        /// Общий коннекшн к DBF
        /// </summary>
        /// <returns> Строка </returns>
        private string CS() {
           return @"Driver={Microsoft Visual FoxPro Driver};" +
                                                     @"SourceType=DBF;" +
                                                     @"SourceDB=" + this.DbfFolder + ";" +
                                                     @"Exclusive=No;" +
                                                     @"CodePage=866";
        }
        public GetData(String Folder)
        {
            DbfFolder = Folder;
        }

        public static string Convert(string value, Encoding src, Encoding trg)
        {
            Decoder dec = src.GetDecoder();
            byte[] ba = trg.GetBytes(value);
            int len = dec.GetCharCount(ba, 0, ba.Length);
            char[] ca = new char[len];
            dec.GetChars(ba, 0, ba.Length, ca, 0);
            return new string(ca);
        }


        public void Print1()
        {
            try
            {
                OleDbConnection conn2 = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;FieldsData Source="+DbfFolder+";Extended Properties=DBASE IV;Persist Security Info=False;");
                //OleDbConnection conn2 = new OleDbConnection(@"Provider=VFPOLEDB.1;FieldsData Source=c:\\books.DBF");
                conn2.Open();
                OleDbCommand comm = conn2.CreateCommand();
                comm.CommandText = "SELECT NAME FROM KLADR WHERE GNINMB='0105'";
                comm.CommandType = CommandType.Text;
                OleDbDataReader reader = comm.ExecuteReader();

                Console.WriteLine("rows:"+reader.HasRows); //true
                Console.WriteLine("fields:"+reader.FieldCount); // 1 
                //Encoding ASCII = Encoding.Default;
               // Encoding cp866 = Encoding.GetEncoding(866);
               // Console.OutputEncoding = ASCII;
                while (reader.Read())
                {
                  //  Console.WriteLine(reader[0]);
                    string b = reader.GetString(0); // Первое поле

                     Console.WriteLine(Convert( b, Encoding.GetEncoding(866), Encoding.Default));

                    //byte[] cp866byte = cp866.GetBytes(b);
                    //byte[] AN = Encoding.Convert(cp866, ASCII, cp866byte);

                    // Console.WriteLine(Encoding.Default.GetString(AN));
                    //  Console.WriteLine(b);

                    // Console.WriteLine(Encoding.GetEncoding(866).GetString(Encoding.Default.GetBytes(b)));
                    //byte[] pValue = Encoding.GetEncoding(866).GetBytes(b);
                    //Console.WriteLine( Encoding.GetEncoding("CP866").GetString(pValue, 0, pValue.Length) );

                    // byte[] s = Encoding.GetEncoding(866).GetBytes(b);

                    // Console.WriteLine( Encoding.ASCII.GetString(s));
                }
            }
            catch (OleDbException exception)
            {
                for (int i = 0; i < exception.Errors.Count; i++)
                    Console.WriteLine("Index #" + i + "\n" +
                     "Message: " + exception.Errors[i].Message + "\n" +
                     "Native: " + exception.Errors[i].NativeError.ToString() + "\n" +
                     "Source: " + exception.Errors[i].Source + "\n" +
                     "SQL: " + exception.Errors[i].SQLState + "\n");

            }
            Console.ReadKey();
        }

        public void Print2()
        {
            OdbcConnection oConn = new OdbcConnection();
            oConn.ConnectionString = CS();
            try
            {
                oConn.Open();

                DataTable dt = new DataTable();
                OdbcCommand oCmd = oConn.CreateCommand();
                oCmd.CommandText = @"SELECT NAME FROM KLADR WHERE GNINMB='0105'";
                dt.Load(oCmd.ExecuteReader());
                DataRowCollection rows = dt.Rows;
                string value;
                foreach (DataRow row in rows)
                {
                    value = row.Field<String>(0);
                    OldLogger.Log(Convert( value, Encoding.GetEncoding(866), Encoding.Default));

                }


            }
            catch (Exception e)
            {
                OldLogger.Log(e.Message);
            }


        }
    }
}
