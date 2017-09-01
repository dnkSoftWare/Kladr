using System.ServiceProcess;
using System.Threading;

namespace KladrService
{
    public partial class KladrService : ServiceBase
    {
        private volatile bool isRunning = false;
//        private Thread thread = null;

        public KladrService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
//            if (isRunning == false && thread == null)
//            {
//                isRunning = true;
//                thread = new Thread(DoWork) { IsBackground = true };
//                thread.Start();
//            }
          InitWork();
            Lk.Start(true);
            Log.Instance.WriteLine("KladrService Started!");
        }

        protected override void OnStop()
        {
            Lk.Stop();
            Log.Instance.WriteLine("KladrService Stopped!");
            //            if (isRunning == true)
            //            {
            //                isRunning = false;
            //                thread.Join(30000);
            //                thread = null;
            //            }
        }
    }
}
