using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտից քարտ փոխանցման կարգավիճակ
    /// </summary>
    public class C2CTransferStatusResponse
    {

        /// <summary>
        /// Քարտից քարտ փոխանցման կարգավիճակի հարցմնան արդյունքի կոդ
        /// </summary>
        public byte ResultCode { get; set; }

        /// <summary>
        /// Քարտից քարտ փոխանցման կարգավիճակի հարցմնան արդյունքի նկարագրություն
        /// </summary>
        public string ResultDescription { get; set; }

        /// <summary>
        /// Քարտից քարտ փոխանցման կարգավիճակի հարցմնան արդյունքի կոդ
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Քարտից քարտ փոխանցման կարգավիճակի հարցմնան արդյունքի նկարագրություն
        /// </summary>
        public string ResponseCodeDescription { get; set; }

        /// <summary>
        /// Տվյալների բազայում փոխանցման հերթական համար
        /// </summary>
        public long TransferID { get; set; }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում հայտի համարով
        /// </summary>
        /// <param name="transferID">Գործարքի նույնականացուցիչը համակարգում </param>
        /// <returns></returns>
        /// 
        public static C2CTransferStatusResponse GetC2CTransferStatus(long transferID)
        {

            C2CTransferStatusResponse response = new C2CTransferStatusResponse();
            response.TransferID = transferID;
            try
            {
                if (transferID != 0)
                {
                    TransactionStatusRequest req = new TransactionStatusRequest();
                    req.ExtensionID = (ulong)transferID;

                    TransactionDetailsBResponse r = ArcaDataService.Check(req);
                    response.ResultCode = (byte)r.ResultCode;
                    response.ResponseCode = r.ResponseCode;
                    response.ResponseCodeDescription = r.ResponseCodeDescription;
                }
                else
                {
                    response.ResultCode = 0;
                    response.ResponseCode = "9999";
                    response.ResponseCodeDescription = "Data not found";
                }
            }
            catch (Exception ex)
            {
                response.ResultCode = 1;
                response.ResponseCode = "9999";
                response.ResponseCodeDescription = "Ծառայությունը հասանելի չէ/Service not available" + ex.Message;
                C2CTransferStatusException c2cex = new C2CTransferStatusException(ex.Message);
                c2cex.TransferStatusResponse = response;
                throw (ex);
            }


            return response;
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում տերմինալի կողմից գեներացված գործարքի համարով
        /// </summary>
        /// <param name="orderID">Վճարային տերմինալի կողմից գեներացված գործարքի նույնականացուցիչ</param>
        /// <returns></returns>
        public static C2CTransferStatusResponse GetC2CTransferStatusByOrderID(long orderID, int sourceID)
        {
            C2CTransferStatusResponse response = new C2CTransferStatusResponse();
            long transferID;
            transferID = C2CTransferOrderDB.GetC2CTransferIDFromOrderID(orderID);
            response = GetC2CTransferStatus(transferID);
            //return C2CTransferOrderDB.GetC2CTransferStatusByOrderID(orderID, sourceID);
            return response;
        }
    }
}
