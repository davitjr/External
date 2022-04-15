using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using opService = ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    /// <summary>
    /// ՃՈ խախտումներ
    /// </summary>
    public class VehicleViolationResponse
    {
        /// <summary>
        /// Ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Հարցման համար
        /// </summary>
        public long ResponseId { get; set; }

        /// <summary>
        /// Որոշման N
        /// </summary>
        public string ViolationNumber { get; set; }

        /// <summary>
        /// Որոշման ա/թ
        /// </summary>
        public DateTime ViolationDate { get; set; }

        /// <summary>
        /// Տույժի գումար
        /// </summary>
        public double PenaltyAmount { get; set; }

        /// <summary>
        /// Տուգանքի գումար
        /// </summary>
        public double FineAmount { get; set; }

        /// <summary>
        /// Ընդամենը պարտք
        /// </summary>
        public double PayableAmount { get; set; }

        /// <summary>
        /// Պարտքի մնացորդ
        /// </summary>
        public double RequestedAmount { get; set; }

        /// <summary>
        /// Մարված գումար
        /// </summary>
        public double PayedAmount { get; set; }

        /// <summary>
        /// Ոստիկանության հաշվեհամար
        /// </summary>
        public string PoliceAccount { get; set; }

        /// <summary>
        /// ՏՄ անձնագիր
        /// </summary>
        public string VehiclePassport { get; set; }

        /// <summary>
        /// ՏՄ համար
        /// </summary>
        public string VehicleNumber { get; set; }

        /// <summary>
        /// ՏՄ տեսակ
        /// </summary>
        public string VehicleModel { get; set; }


        /// <summary>
        /// Վերադարձնում է ՃՈ կատարած հարցումները
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        internal static List<VehicleViolationResponse> GetVehicleViolationResponses(long responseId)
        {

            return VehicleViolationResponseDB.GetVehicleViolationResponses(responseId);
        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտումները խախտման համարով
        /// </summary>
        /// <param name="violationId"></param>
        /// <returns></returns>
        public static List<VehicleViolationResponse> GetVehicleViolationById(string violationId, ACBAServiceReference.User user)
        {
            long responseId;
            opService.ViolationRequestResponse response = ACBAOperationService.GetVehicleViolationById(violationId, user);
            responseId = response.ResponseId;

            return VehicleViolationResponse.GetVehicleViolationResponses(responseId);
        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտումները մեքենայի համարով և տեխ. զննման համարով
        /// </summary>
        /// <param name="violationId"></param>
        /// <returns></returns>
        public static List<VehicleViolationResponse> GetVehicleViolationByPsnVehNum(string psn, string vehNum, ACBAServiceReference.User user)
        {
            long responseId;
            opService.ViolationRequestResponse response = ACBAOperationService.GetVehicleViolationByPsnVehNum(psn, vehNum, user);
            responseId = response.ResponseId;

            return VehicleViolationResponse.GetVehicleViolationResponses(responseId);
        }

        /// <summary>
        /// Վերադարձնում է ՃՈ խախտուման հարցման պատասխանը
        /// </summary>
        /// <param name="id">Հարցման արդյունքում ստացված պատասխանի ունիկալ համար</param>
        /// <returns></returns>
        public static VehicleViolationResponse GetVehicleViolationResponseDetails(long id)
        {
            return VehicleViolationResponseDB.GetVehicleViolationResponseDetails(id);
        }

    }
}
