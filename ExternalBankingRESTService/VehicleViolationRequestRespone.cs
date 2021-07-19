using ExternalBankingRESTService.XBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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