using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class VehicleViolationRequestRespone
    {
        public List<VehicleViolationResponse> VehicleViolations { get; set; }
        public Result Result { get; set; }

        public VehicleViolationRequestRespone()
        {
            Result = new Result();
        }

    }
}