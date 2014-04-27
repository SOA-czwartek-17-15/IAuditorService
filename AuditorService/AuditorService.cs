using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuditorService
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceRepository = "net.tcp://localhost:54321/ServiceRepo";//to powinno byc odczytane z app.config
            //strworzenie clienta do service repo
            //zarejestrowanie swojej uslugi
            //odpalenie timera w celu wysylania Alive
            //stworznie AccountRepository i przekazanie mu serviceRepository zeby odpytac o potrzebne sewisy

            // logowanie
        }
    }
    
    /*
     * jakas operacja do odczytu/zapisu do DB
     * ORM: Hibernate lub Entity Framework
     * logowanie: log4net
     */
    class AuditorService : IAuditorService
    {
    }
}
