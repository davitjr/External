using ExternalBanking.ContractServiceRef;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    class Contracts
    {
        internal static string RenderContractHTML(string contractName, Dictionary<string, string> parameters, string fileName)
        {
            string fileContent = "";


            try
            {
                Contract contract = new Contract();
                contract.ContractName = contractName;

                contract.ParametersList = new List<StringKeyValue>();

                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> param in parameters)
                    {
                        StringKeyValue oneParam = new StringKeyValue();
                        oneParam.Key = param.Key;
                        oneParam.Value = param.Value;
                        contract.ParametersList.Add(oneParam);
                    }
                }

                using (ContractOerationServiceClient proxy = new ContractOerationServiceClient())
                {

                    fileContent = proxy.DownloadContractHTML(contract, "MB", "0");
                }

                return fileContent;
                //ShowDocument(fileContent, ExportFormat.PDF, fileName);
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                System.Web.HttpContext.Current.Response.StatusCode = 422;
                System.Web.HttpContext.Current.Response.StatusDescription = ex.Message;   //Utility.ConvertUnicodeToAnsi(ex.Message);
                fileContent = ex.Message;
                return fileContent;

            }
        }



        internal static byte[] RenderContract(string contractName, Dictionary<string, string> parameters, string fileName, Contract contract = null)
        {
            byte[] fileContent = null;
            //try
            //{

            if (contract == null)
            {
                contract = new Contract();
                contract.ParametersList = new List<StringKeyValue>();
                contract.ContractName = contractName;
            }


            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    StringKeyValue oneParam = new StringKeyValue();
                    oneParam.Key = param.Key;
                    oneParam.Value = param.Value;
                    contract.ParametersList.Add(oneParam);
                }
            }
            using (ContractOerationServiceClient proxy = new ContractOerationServiceClient())
            {

                fileContent = proxy.DownloadContract(contract, "HB", "0");
            }

            return fileContent;

            //}
            //catch (Exception ex)
            //{

            //    System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
            //    System.Web.HttpContext.Current.Response.StatusCode = 422;
            //    System.Web.HttpContext.Current.Response.StatusDescription = ex.Message;
            //    return fileContent;
            //}


        }


    }
}
