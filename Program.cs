using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Hosting;
using System.ServiceProcess;
using System.Text;

namespace KladrService
{
    static class Program
    {
        static string exePath = typeof(Program).Assembly.Location;
       // public static string[] CurArgs;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Environment.CurrentDirectory = Path.GetDirectoryName(exePath);
           // args.CopyTo(CurArgs,0);

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "-start":
                        Start();
                        break;
                    case "-stop":
                        Stop();
                        break;
                    case "-restart":
                        Restart();
                        break;
                    case "-i":
                        InstallService();
                        break;
                    case "-u":
                        UninstallService();
                        break;
                    case "-console":
                        ServiceBase[] ServicesToRun;
                        ServicesToRun = new ServiceBase[] { new KladrService() };
                        ServiceBase srv = ServicesToRun[0];
                        MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
                        MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
                        onStartMethod.Invoke(srv, new object[] { args });
                        Console.WriteLine("Press escape to terminate...");
                        while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
                        onStopMethod.Invoke(srv, new object[0]);
                        break;
                    default:
                        ShowInfo();
                        break;
                }
            }
            else
            {
                if (System.Diagnostics.Process.GetCurrentProcess().SessionId == 0)
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new KladrService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    ShowInfo();
                }
            }
        }

        private static void ShowInfo()
        {
            Console.WriteLine(
@"-i          - Установка службы
-u          - Удаление службы
-console    - Консольный режим
-start      - Запуск службы
-stop       - Остановка службы
-restart    - Перезапуск службы
");
        }

        public static void Start()
        {
            ServiceController service = GetService();
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine("Service is already running");
                }
                else
                {
                    try
                    {
                        service.Start();
                        Console.WriteLine("Service is started");
                    }
                    catch (Exception ex) { Log.Instance.WriteException(ex); }
                }
            }
        }

        public static void Stop()
        {
            ServiceController service = GetService();
            if (service != null)
            {

                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("Service is already stopped");
                }
                else
                {
                    try
                    {
                        service.Stop();
                        Console.WriteLine("Service is stopped");
                    }
                    catch (Exception ex) { Log.Instance.WriteException(ex); }
                }
            }
        }

        public static void Restart()
        {
            ServiceController service = GetService();
            if (service != null)
            {
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                    }
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                    service.Start();
                    Console.WriteLine("Service is restarted");
                }
                catch (Exception ex) { Log.Instance.WriteException(ex); }
            }
        }

        private static ServiceController GetService()
        {
            ServiceController service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "KladrService");
            if (service == null)
            {
                Console.WriteLine("Service isn't install");
            }
            return service;
        }

        public static void InstallService()
        {
            try
            {
                System.Configuration.Install.AssemblyInstaller Installer = new System.Configuration.Install.AssemblyInstaller(exePath, new string[] { "" });
                Installer.UseNewContext = true;
                Installer.Install(null);
                Installer.Commit(null);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteException(ex);
            }
        }

        public static void UninstallService()
        {
            try
            {
                System.Configuration.Install.AssemblyInstaller Installer = new System.Configuration.Install.AssemblyInstaller(exePath, new string[] { "" });
                Installer.UseNewContext = true;
                Installer.Uninstall(null);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteException(ex);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Instance.WriteException(e.ExceptionObject as Exception);
            Log.Instance.Dispose();
        }
    }
}
