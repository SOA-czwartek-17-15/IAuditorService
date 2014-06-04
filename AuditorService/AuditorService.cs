using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Configuration;
using System.Timers;
using System.Threading;
using Contracts;
using log4net;
using NHibernate;
using NHibernate.Criterion;


namespace AuditorService
{
    class Program
    {
        //for true: the AuditorService won't connect to the ServiceRepository
        private static bool _test = false;
        private static bool _sendAlive = true;

        private static IServiceRepository ServiceRepo { get; set; }

        static void Main(string[] args)
        {
            //logger config
            log4net.Config.XmlConfigurator.Configure();

            Log("Initialising...");

            //Configuration
            string address = "net.tcp://192.168.0.91:" + ConfigurationSettings.AppSettings["port"] + "/IAuditorService";
            AuditorService auditor = new AuditorService();
            ServiceHost sh = new ServiceHost(auditor, new Uri[] { new Uri(address) });
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

            Log("Service has started");

            if (!_test) 
            {
                Log("Registering to repository...");

                //registering
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, new EndpointAddress(ConfigurationSettings.AppSettings["ServiceRepositoryAddress"]));
                ServiceRepo = cf.CreateChannel();

                try
                {
                    ServiceRepo.RegisterService("IAuditorService", address);
                    Log("Service has been registered");
                }
                catch (Exception e)
                {
                    _sendAlive = false;
                    LogException(e.Message);
                    while (!ReRegister()) ;
                }
            }

            //setting up the timer
            SendAliveSignal();

            System.Timers.Timer t = new System.Timers.Timer(1000 * 3); // 1000 milisecond * 60 = 1 minute
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler(Alive);
            t.Start();

            Log("Service has set the timer up");

            Log("Service is ready");

            //auditor.setRepos(ServiceRepo);

            //Log("Set repo for auditor service");
            //auditor.AddAudit("666666", 666666);
            //Audit audit = auditor.GetLastAuditByAccount("666666");

            //Console.WriteLine(audit.Money);

            //at this point the service is ready
            //keypress closes the service
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            if (!_test) 
            { 
                try
                {
                    _sendAlive = false;
                    t.Stop();

                    ServiceRepo.Unregister("IAuditorService");
                    Log("Service has been unregistered");
                }
                catch (Exception e)
                {
                    LogException(e.Message);
                }
            }

            Log("Exiting...");
        }

        private static void Alive(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_sendAlive)
            {
                SendAliveSignal();
            }
        }

        private static void SendAliveSignal()
        {
            if (!_test) 
            { 
                try
                {
                    ServiceRepo.Alive("IAuditorService");
                    Log("'Alive' signal sent");
                }
                catch (Exception e)
                {
                    _sendAlive = false;

                    LogException("Connection lost");
                    LogException(e.Message);

                    while (!ReRegister()) ;
                }
            }
        }

        private static bool ReRegister()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, new EndpointAddress(ConfigurationSettings.AppSettings["ServiceRepositoryAddress"]));
                ServiceRepo = cf.CreateChannel();

                Log("Connected.");

                ServiceRepo.RegisterService("IAuditorService", "net.tcp://192.168.0.91:" + ConfigurationSettings.AppSettings["port"] + "/IAuditorService");
                _sendAlive = true;

                Log("Registed.");
                
                return true;
            }
            catch (EndpointNotFoundException ex)
            {
                LogException("Could not connect. Trying again.");
                System.Threading.Thread.Sleep(5000);
            }

            return false;
        }

        private static void Log(String log)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine(now.ToString("HH:mm:ss") + ": " + log);
            Logger.log.Info(log);
        }

        private static void LogException(String log)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine(now.ToString("HH:mm:ss") + ": " + log);
            Logger.log.Error(log);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class AuditorService : IAuditorService
    {
        private static IServiceRepository ServiceRepo { get; set; }
        private static IAccountRepository AccountRepo { get; set; }

        public void setRepos(IServiceRepository serviceRepo)
        {
            ServiceRepo = serviceRepo;

            string location = ServiceRepo.GetServiceLocation("IAccountService");



            NetTcpBinding binding2 = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(binding2, new EndpointAddress(location));
            AccountRepo = cf2.CreateChannel();
        }

	    public Audit GetLastAuditByAccount(string accountNumber)
        {
            Log("Getting audit");
            try
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        var c = session.CreateCriteria<Audit>();
                        c.Add(Expression.Eq("AccountNumber", accountNumber));
                        Audit audit = c.UniqueResult<Audit>();
                        Log("Got audit");
                        return audit;
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return null;
            }

            return null;
	    }

	public IEnumerable<Audit> GetAllAuditsByAccount(string accountNumber)
	{/*
		Log("Service has started");

            if (!_test) 
            {
                Log("Registering to repository...");

                //registering
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, new EndpointAddress(ConfigurationSettings.AppSettings["ServiceRepositoryAddress"]));
                ServiceRepo = cf.CreateChannel();

                try
                {
                    string location = ServiceRepo.GetServiceLocation("IAccountRepository");
                }
                catch (Exception e)
                {
                     LogError("Fatal exception: cannot get service location");
                }
            }

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(binding, new EndpointAddress(location));    
	    ServiceRepo2 = cf2.CreateChannel();	    
        */
	    return null;
	}

	public IEnumerable<Audit> GetAllAudits()
	{/*
	    Log("Service has started");

            if (!_test) 
            {
                Log("Registering to repository...");

                //registering
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, new EndpointAddress(ConfigurationSettings.AppSettings["ServiceRepositoryAddress"]));
                ServiceRepo = cf.CreateChannel();

                try
                {
                    string location = ServiceRepo.GetServiceLocation("IAccountRepository");
                }
                catch (Exception e)
                {
                     log.Error("Fatal exception: cannot get service location", e);
                }
            }

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IAccountRepository> cf2 = new ChannelFactory<IAccountRepository>(binding, new EndpointAddress(location));    
	    ServiceRepo2 = cf2.CreateChannel();	    
        */
	    return null;
	}

        public bool AddAudit(String accountNumber, long Money)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        Audit audit = new Audit();
                        audit.AccountNumber = accountNumber;
                        audit.Money = Money;
                        audit.Date = DateTime.Now;

                        session.Save(audit);
                        transaction.Commit();

                        Log("Created new Audit for Account number: " + accountNumber);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return false;
            }
        }

        private void Log(String log)
        {
            Logger.log.Info(log);
            Console.WriteLine(log);
        }

        private void LogError(String log)
        {
            Logger.log.Error(log);
            Console.WriteLine(log);
        }
    }

    public class Logger
    {
        internal static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
