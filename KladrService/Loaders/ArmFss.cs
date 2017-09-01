using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;



namespace KladrService.Loaders
{
   public class ArmFss
   {
       public bool FlagExecuted { get; set; } = false;
        
        public ArmFss(string pathToKladrDbf, string pathToAddresFdb)
        {
            var tasks = new List<Task>();
            var factory = new TaskFactory(
                                            TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent,
                                            TaskContinuationOptions.None);

            tasks.Add(factory.StartNew(() =>
            {
                //var fileName = PrepareKLADR(pathToKladrDbf);
                //ExecuteSql(fileName, pathToAddresFdb); 
                Console.WriteLine("KLADR");
            }));
            
            tasks.Add(factory.StartNew(() =>
            {
               /* var fileName = PrepareSTREET(pathToKladrDbf);
                ExecuteSql(fileName, pathToAddresFdb); */
                Console.WriteLine("STREET");
            }));
            tasks.Add(factory.StartNew(() =>
            {
                var fileName = PrepareDOMA(pathToKladrDbf);
               // ExecuteSql(fileName, pathToAddresFdb); 
                Console.WriteLine("DOMA");
            }));
            
            var finalTask = factory.ContinueWhenAll(tasks.ToArray(), myTasks => {
                int nSuccessfulTasks = 0;
                int nFailed = 0;

                foreach (var t in myTasks)
                {
                    if (t.Status == TaskStatus.RanToCompletion/* && t.Exception == null*/)
                        nSuccessfulTasks++;

                    if (t.Status == TaskStatus.Faulted/* || t.Exception != null*/)
                        nFailed++;
                }
                
                if (nFailed > 0)
                {
                    Log.Instance.WriteError("{0} tasks failed.", nFailed);
                }
                if(nSuccessfulTasks == tasks.Count)
                {
                    Log.Instance.WriteLine("Обновление ADDRES.FDB выполнена успешно!");
                    FlagExecuted = true;
                }
            });
            finalTask.Wait();


        }


        private string PrepareDOMA(string pathToKladrDbf)
        {
            var oConn = DbfConnection(pathToKladrDbf);

            DataTable dt = new DataTable();
            OdbcCommand oCmd = oConn.CreateCommand();
            oCmd.CommandText = @"SELECT NAME, SOCR, CODE, INDEX, GNINMB FROM DOMA";
            dt.Load(oCmd.ExecuteReader());
            //  DataRowCollection rows = dt.Rows;
            var domatable = dt.AsEnumerable().AsParallel().
                Select(l => new
                {
                    name = l.Field<string>("NAME").ConvertFrom866().TrimEnd(' '),
                    socr = l.Field<string>("SOCR").ConvertFrom866().TrimEnd(' '),
                    code = l.Field<string>("CODE"),
                    actcode = l.Field<string>("CODE").ActualCode(),
                    typecode = l.Field<string>("CODE").TypeCode(),
                    index = l.Field<string>("INDEX").TrimEnd(' '),
                    sninmb = l.Field<string>("GNINMB").TrimEnd(' ')
                }).
                Where(l => !l.code.Is51or99CODE())
                .Take(120)
                ;


            var fileName = Directory.GetCurrentDirectory() + pathToKladrDbf + @"\doma.sql";

            using (var file = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                file.WriteLine("DELETE from addr6;");
                file.WriteLine("END^");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                var term_b = "";
                var term_e = "";
                var cnt = 1;
                foreach (var kladr in domatable)
                {
                    if (cnt % 100 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else { term_b = ""; term_e = ""; }
                    file.WriteLine($@"INSERT INTO ADDR6 (KODREG, KODRAI, KODTOW, KODNAS, KODULI, KODDOM, VER, NAME, SOCR, PINDEX, GNI)
VALUES('{kladr.code.GetPCode(1)}', '{kladr.code.GetPCode(2)}', '{kladr.code.GetPCode(3)}', '{kladr.code.GetPCode(4)}', '{kladr.code.GetPCode(5)}', '{kladr.code.GetPCode(6)}', {kladr.actcode}, '{kladr.name}', '{kladr.socr}', '{kladr.index}', '{kladr.sninmb}');{term_e}{term_b}");
                    cnt++;
                }

                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
            }
            return fileName;
        }

        private string PrepareSTREET(string pathToKladrDbf)
        {
            var oConn = DbfConnection(pathToKladrDbf);

            DataTable dt = new DataTable();
            OdbcCommand oCmd = oConn.CreateCommand();
            oCmd.CommandText = @"SELECT * FROM STREET";
            dt.Load(oCmd.ExecuteReader());
            //  DataRowCollection rows = dt.Rows;
            var streettable = dt.AsEnumerable().AsParallel().
                Select(l => new
                {
                    name = l.Field<string>("NAME").ConvertFrom866().TrimEnd(' '),
                    socr = l.Field<string>("SOCR").ConvertFrom866().TrimEnd(' '),
                    code = l.Field<string>("CODE"),
                    actcode = l.Field<string>("CODE").ActualCode(),
                    typecode = l.Field<string>("CODE").TypeCode(),
                    index = l.Field<string>("INDEX").TrimEnd(' '),
                    sninmb = l.Field<string>("GNINMB").TrimEnd(' ')
                }).
                Where(l => !l.code.Is51or99CODE())
                .Take(120)
                ;


            var fileName = Directory.GetCurrentDirectory() + pathToKladrDbf + @"\street.sql";

            using (var file = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                file.WriteLine("DELETE from addr5;");
                file.WriteLine("END^");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                var term_b = "";
                var term_e = "";
                var cnt = 1;
                foreach (var kladr in streettable)
                {
                    if (cnt % 100 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else { term_b = ""; term_e = ""; }
                    file.WriteLine($@"INSERT INTO ADDR5 (KODREG, KODRAI, KODTOW, KODNAS, KODULI, VER, NAME, SOCR, PINDEX, GNI)
VALUES('{kladr.code.GetPCode(1)}', '{kladr.code.GetPCode(2)}', '{kladr.code.GetPCode(3)}', '{kladr.code.GetPCode(4)}', '{kladr.code.GetPCode(5)}', {kladr.actcode}, '{kladr.name}', '{kladr.socr}', '{kladr.index}', '{kladr.sninmb}');{term_e}{term_b}");
                    cnt++;
                }

                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
            }
            return fileName;
        }

        private static string PrepareKLADR(string pathToKladrDbf)
        {
            var oConn = DbfConnection(pathToKladrDbf);

            DataTable dt = new DataTable();
            OdbcCommand oCmd = oConn.CreateCommand();
            oCmd.CommandText = @"SELECT * FROM KLADR";
            dt.Load(oCmd.ExecuteReader());
          //  DataRowCollection rows = dt.Rows;
            var kladrtable = dt.AsEnumerable().AsParallel().
                Select(kl => new
                {
                    name = kl.Field<string>("NAME").ConvertFrom866().TrimEnd(' '),
                    socr = kl.Field<string>("SOCR").ConvertFrom866().TrimEnd(' '),
                    code = kl.Field<string>("CODE"),
                    actcode = kl.Field<string>("CODE").ActualCode(),
                    typecode = kl.Field<string>("CODE").TypeCode(),
                    index = kl.Field<string>("INDEX").TrimEnd(' '),
                    sninmb = kl.Field<string>("GNINMB").TrimEnd(' ')
                }).
                Where(kl => !kl.code.Is51or99CODE())
                .Take(120) 
                ;
           

            var fileName = Directory.GetCurrentDirectory() + pathToKladrDbf + @"\kladr.sql";

            using (var file = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                file.WriteLine("DELETE from addr1;");
                file.WriteLine("DELETE from addr2;");
                file.WriteLine("DELETE from addr3;");
                file.WriteLine("DELETE from addr4;");
                file.WriteLine("END^");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                var term_b = "";
                var term_e = "";
                var cnt = 1;
                foreach (var kladr in kladrtable)
                {
                    if (cnt % 100 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else { term_b = ""; term_e = ""; }
                    file.WriteLine(string.Format(@"INSERT INTO ADDR{9} (KODREG, KODRAI, KODTOW, KODNAS, VER, NAME, SOCR, PINDEX, GNI)
VALUES('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', '{8}');{11}{10}",
                                                               kladr.code.GetPCode(1), kladr.code.GetPCode(2), kladr.code.GetPCode(3), kladr.code.GetPCode(4),
                                                               kladr.actcode, kladr.name, kladr.socr, kladr.index, kladr.sninmb,
                                                               kladr.typecode, term_b, term_e
                                                               ));
                    cnt++;
                }

                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
            }
            return fileName;
        }

        private static OdbcConnection DbfConnection(string pathToKladrDbf)
        {
            var dbfFolder = Directory.GetCurrentDirectory() + pathToKladrDbf;
            var oConn = new OdbcConnection();
            oConn.ConnectionString = @"Driver={Microsoft Visual FoxPro Driver};" +
                                     @"SourceType=DBF;" +
                                     @"SourceDB=" + dbfFolder + ";" +
                                     @"Exclusive=No;" +
                                     @"CodePage=866";

            oConn.Open();
            return oConn;
        }

        private static void ExecuteSql(string fileName, string pathToAddresFdb)
        {
            var sqlFileName = fileName;
            try
            {

                var cs = new FbConnectionStringBuilder
                {
                    DataSource = "localhost",
                    Database = Directory.GetCurrentDirectory() + pathToAddresFdb + @"\ADDRESS.FDB",
                    UserID = "SYSDBA",
                    Password = "masterkey",
                    Dialect = 1,
                    Charset = "WIN1251",
                    ServerType = FbServerType.Default
                };

                var conn = new FbConnection(cs.ToString());
                conn.Open();
                // FbDatabaseInfo fb_inf = new FbDatabaseInfo(connection); //информация о БД
                var be = new FbBatchExecution(conn);

                var script = "";
                using (var sr = new StreamReader(sqlFileName, Encoding.GetEncoding(1251)))
                {
                    script = sr.ReadToEnd();
                }

                var fbs = new FbScript(script);
                fbs.Parse();
                be.AppendSqlStatements(fbs);
                be.Execute(autoCommit: true);
                conn.Close();
                Log.Instance.WriteLine("Script {0} loaded!", fileName);
                
            }
            catch (Exception ex)
            {
              Log.Instance.WriteError("Error in script:{0} \n {1}",fileName, ex.Message);
                throw;
            }
            finally
            {
              
            }
        }
    }
}
