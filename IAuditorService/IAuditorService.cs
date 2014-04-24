using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace IAuditorService
{
    /*
     * jakas operacja do odczytu/zapisu do DB
     * ORM: Hibernate lub Entity Framework
     */
    [ServiceContract]
    interface IAuditorService
    {
        [OperationContract]
        int GetNumberOfTransfers(DateTime date);
        [OperationContract]
        int GetTransferedMoney(DateTime date);
        [OperationContract]
        int GetCreditNumber();
        [OperationContract]
        bool AuditAll();
        [OperationContract]
        bool GetHistory(); //TODO: jakis format historii, zeby zwracac info
    }
}
