using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;



namespace KladrService.Loaders
{
    public class ArmFss
    {
        private string PathToKladrDbf { get; }
        private string PathToAddresFdb { get; }
        public bool FlagExecuted { get; set; } = false;
        public object FdbName { get; }

        public ArmFss(string pathToKladrDbf, string pathToAddresFdb, string fdbName)
        {
            PathToKladrDbf = pathToKladrDbf;
            PathToAddresFdb = pathToAddresFdb;
            FdbName = fdbName;
        }

        public void ImportData()
        {
            var tasks = new List<Task>();
            var factory = new TaskFactory(
                TaskCreationOptions.LongRunning,
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
                    ExecuteSql(CreateDeleteScript("addr6"));
                    Console.WriteLine("CLEAR DOMA");
                    for (int i = 0; i < 3; i++)
                    {
                        ExecuteSql(PrepareDOMA(i, 1000000));
                        Console.WriteLine($"DOMA{i}");
                    }
//                    ExecuteSql(PrepareDOMA(1, 100000));
//                    Console.WriteLine($"DOMA{1}");
//                    ExecuteSql(PrepareDOMA(2, 100000));
//                    Console.WriteLine($"DOMA{2}");
                }));
//            }

            var finalTask = factory.ContinueWhenAll(tasks.ToArray(), myTasks =>
            {
                int nSuccessfulTasks = 0;
                int nFailed = 0;

                foreach (var t in myTasks)
                {
                    if (t.Status == TaskStatus.RanToCompletion /* && t.Exception == null*/)
                        nSuccessfulTasks++;

                    if (t.Status == TaskStatus.Faulted /* || t.Exception != null*/)
                        nFailed++;
                }

                if (nFailed > 0)
                {
                    Log.Instance.WriteError("{0} tasks failed.", nFailed);
                    FlagExecuted = false;
                }
                if (nSuccessfulTasks == tasks.Count)
                {
                    Log.Instance.WriteLine("Добавление данных в ADDRES.FDB выполнено успешно!");
                    FlagExecuted = true;
                }
            });
            finalTask.Wait();
        }

        public void ClearTables()
        {
            var tasks = new List<Task>();
            var factory = new TaskFactory(
                TaskCreationOptions.LongRunning,
                TaskContinuationOptions.None);

            for (int i = 1; i <= 6; i++)
            {
                tasks.Add(factory.StartNew(() =>
                {
                    var filename = CreateDeleteScript($"addr{i}");
                    ExecuteSql(filename);
                }));
            }
            var finalTask = factory.ContinueWhenAll(tasks.ToArray(), myTasks =>
                {
                    int nSuccessfulTasks = 0;
                    int nFailed = 0;

                    foreach (var t in myTasks)
                    {
                        if (t.Status == TaskStatus.RanToCompletion /* && t.Exception == null*/)
                            nSuccessfulTasks++;

                        if (t.Status == TaskStatus.Faulted /* || t.Exception != null*/)
                            nFailed++;
                    }

                    if (nFailed > 0)
                    {
                        Log.Instance.WriteError("{0} tasks failed.", nFailed);
                        FlagExecuted = false;
                    }
                    if (nSuccessfulTasks == tasks.Count)
                    {
                        Log.Instance.WriteLine("Удаление данных из таблиц ADDRES.FDB выполнено успешно!");
                        FlagExecuted = true;
                    }
                });
                finalTask.Wait();
            }

  
public string CreateDeleteScript(string tableName)
        {
            var fileName = Directory.GetCurrentDirectory() + PathToKladrDbf + $@"\{tableName}.sql";

            using (var file = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                file.WriteLine($@"DELETE from {tableName};");
                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
            }
            return fileName;
        }
public string PrepareDOMA(int cntFile, int part_cnt)
        {
            var oConn = DbfConnection(PathToKladrDbf);
            int iterac = 0;
            FlagExecuted = false;
  
            OdbcCommand oCmd = oConn.CreateCommand();
            oCmd.CommandText = @"SELECT NAME, SOCR, CODE, INDEX, GNINMB FROM DOMA";

            OdbcDataReader reader = oCmd.ExecuteReader();

            var lstDatas = new List<TData>();
            TData d; 
            if (reader.HasRows)
            {
                while (iterac < cntFile * part_cnt) // пропускаем необходимое кол-во
                {
                    reader.Read(); iterac++;
                }
                Console.WriteLine($"was skiped {iterac}");
                while (reader.Read() && lstDatas.Count < part_cnt)
                {                  
                    d.Name = reader.GetString(0);
                    d.Socr = reader.GetString(1);
                    d.Code = reader.GetString(2);
                    d.Index = reader.GetString(3);
                    d.Gninmb = reader.GetString(4);
                   // if(!d.Code.Is51or99CODE())
                     lstDatas.Add(d);                    
                }
                Console.WriteLine($"added from {cntFile} - {lstDatas.Count}");
            }
                reader.Close();

            var domatable = lstDatas.AsParallel().
                Select(l => new
                    {
                        name = l.Name.ConvertFrom866().TrimEnd(' '),
                        socr = l.Socr.ConvertFrom866().TrimEnd(' '),
                        code = l.Code,
                        actcode = l.Code.ActualCode(),
                        typecode = l.Code.TypeCode(),
                        index = l.Index.TrimEnd(' '),
                        sninmb = l.Gninmb.TrimEnd(' ')

                    })
                     .Where(l => !l.code.Is51or99CODE())
                    // .Take(120)
                ;
            lstDatas = null;

            var fileName = Directory.GetCurrentDirectory() + PathToKladrDbf + $@"\doma{cntFile}.sql";

            using (var file = new System.IO.StreamWriter(fileName,false,Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                var term_b = "";
                var term_e = "";
                var cnt = 1;
                foreach (var kladr in domatable)
                {
                    if (cnt % 250 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else
                    {
                        term_b = "";
                        term_e = "";
                    }
                    file.WriteLine(
                        $@"INSERT INTO ADDR6 (KODREG, KODRAI, KODTOW, KODNAS, KODULI, KODDOM, NAME, SOCR, PINDEX, GNI)
        VALUES('{kladr.code.GetPCode(1)}', '{kladr.code.GetPCode(2)}', '{kladr.code.GetPCode(3)}', '{kladr.code.GetPCode(4)
        }', '{kladr.code.GetPCode(5)}', '{kladr.code.GetPCode(6)}', '{kladr.name}', '{kladr.socr}', '{kladr.index
        }', '{kladr.sninmb}');{term_e}{term_b}");
                    cnt++;
                }

                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
               
            }
            FlagExecuted = true;
            return fileName;
        }

private string PrepareSTREET()
        {
            var oConn = DbfConnection(PathToKladrDbf);

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


            var fileName = Directory.GetCurrentDirectory() + PathToKladrDbf + @"\street.sql";

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
                    if (cnt%100 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else
                    {
                        term_b = "";
                        term_e = "";
                    }
                    file.WriteLine(
                        $@"INSERT INTO ADDR5 (KODREG, KODRAI, KODTOW, KODNAS, KODULI, VER, NAME, SOCR, PINDEX, GNI)
VALUES('{kladr.code.GetPCode(1)}', '{kladr.code.GetPCode(2)}', '{kladr.code.GetPCode(3)}', '{kladr.code.GetPCode(4)}', '{kladr
                            .code.GetPCode(5)}', {kladr.actcode}, '{kladr.name}', '{kladr.socr}', '{kladr.index}', '{kladr
                            .sninmb}');{term_e}{term_b}");
                    cnt++;
                }

                file.WriteLine("END^");
                file.WriteLine("SET TERM ; ^");
            }
            return fileName;
        }

private string PrepareKLADR()
        {
            var oConn = DbfConnection(PathToKladrDbf);

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


            var fileName = Directory.GetCurrentDirectory() + PathToKladrDbf + @"\kladr.sql";

            using (var file = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(1251)))
            {
                file.WriteLine("SET TERM ^ ;");
                file.WriteLine("EXECUTE BLOCK AS BEGIN");
                var term_b = "";
                var term_e = "";
                var cnt = 1;
                foreach (var kladr in kladrtable)
                {
                    if (cnt%100 == 0)
                    {
                        term_b = "EXECUTE BLOCK AS BEGIN";
                        term_e = "\nEND^\n";
                    }
                    else
                    {
                        term_b = "";
                        term_e = "";
                    }
                    file.WriteLine(
                        string.Format(@"INSERT INTO ADDR{9} (KODREG, KODRAI, KODTOW, KODNAS, VER, NAME, SOCR, PINDEX, GNI)
VALUES('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', '{8}');{11}{10}",
                            kladr.code.GetPCode(1), kladr.code.GetPCode(2), kladr.code.GetPCode(3),
                            kladr.code.GetPCode(4),
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

public void ExecuteSql(string fileName)
        {
            var sqlFileName = fileName;
            FlagExecuted = false;
            try
            {

                var cs = new FbConnectionStringBuilder
                {
                    DataSource = "localhost",
                    Database = Directory.GetCurrentDirectory() + PathToAddresFdb + @"\" + FdbName,
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
                FlagExecuted = true;
            }
            catch (Exception ex)
            {
                Log.Instance.WriteError("Error in script:{0} \n {1}", fileName, ex.Message);
                FlagExecuted = false;
                throw;
            }
            finally
            {

            }
        }

        
    }

        internal struct TData
        {
            public string Name;
            public string Socr;
            public string Code;
            public string Index;
            public string Gninmb;

        }
    }

