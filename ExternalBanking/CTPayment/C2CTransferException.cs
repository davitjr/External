using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class C2CTransferException:Exception
    {
        public C2CTransferResult TransferResponse { get; set; }

        public C2CTransferException(string msg) : base(msg)
        {

        }

    }

    public class C2CTransferStatusException : Exception
    {
        public C2CTransferStatusResponse TransferStatusResponse { get; set; }

        public C2CTransferStatusException(string msg) : base(msg)
        {

        }

    }
}
