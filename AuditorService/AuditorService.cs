using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using Contracts;
using log4net;
using System.Timers;

namespace AuditorService
{
    class Program
    {
        static void Main(string[] args)
        {
            //logger config
            log4net.Config.XmlConfigurator.Configure();

            log("Initializing...");

            //Configuration
            AuditorService auditor = new AuditorService();
            ServiceHost sh = new ServiceHost(auditor, new Uri[] { new Uri("net.tcp://localhost:54390/IAuditorService") });
            sh.AddServiceEndpoint(typeof(IAuditorService), new NetTcpBinding(SecurityMode.None), "net.tcp://0.0.0.0:54390/IAuditorService");
            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }
            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            sh.Open();

            log("Service has started");

            //registering

            log("Service has been registered");

            Timer t = new Timer(1000 * 60 * 10); // 1000 milisecond * 60 = 1 minute
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler(Alive);
            t.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            log("Exiting...");

            //string serviceRepository = "net.tcp://localhost:54321/ServiceRepo";//to powinno byc odczytane z app.config
            //strworzenie clienta do service repo
            //zarejestrowanie swojej uslugi
            //odpalenie timera w celu wysylania Alive
            //stworznie AccountRepository i przekazanie mu serviceRepository zeby odpytac o potrzebne sewisy

            // logowanie
        }

        private static void Alive(object sender, System.Timers.ElapsedEventArgs e)
        {
            //TODO send isAlive info
            log("'Alive' signal sent");
        }

        private static void log(String log)
        {
            Console.WriteLine(log);
            Logger.log.Info(log);
        }
    }
    
    /*
     * jakas operacja do odczytu/zapisu do DB
     * ORM: Hibernate lub Entity Framework
     * logowanie: log4net
     */
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    class AuditorService : IAuditorService
    {
        public int GetNumberOfTransfersByDate(DateTime date)
        {
            return 0;
        }

        public int GetNumberOfTransfersByAccount(Guid accountId)
        {
            return 0;
        }

        public int GetTransferedMoneyByDate(DateTime date)
        {
            return 0;
        }

        public int GetTransferedMoneyByAccount(Guid accountId)
        {
            return 0;
        }

        public int GetNumberOfCredits()
        {
            return 0;
        }

        public IEnumerable<Audit> AuditAll()
        {
            return null;
        }

        public IEnumerable<Audit> GetAuditsByDate(DateTime date)
        {
            return null;
        }

        public IEnumerable<Audit> GetAuditsByAccount(Guid accountId)
        {
            return null;
        }

        public bool AddAudit(Guid accountId, long Money)
        {
            return false;
        }
    }

    public class Logger
    {
        internal static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
