using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.ContractServiceReference;

namespace ExternalBankingRESTService
{
    public class Contracts
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


        //public static void ShowDocument(byte[] fileContent, ExportFormat exportFormat, string fileName)
        //{
        //    //DOWNLOAD FILE
        //    string format = GetExportExtensionString(exportFormat);
        //    System.Web.HttpContext.Current.Response.Clear();
        //    System.Web.HttpContext.Current.Response.ClearContent();
        //    System.Web.HttpContext.Current.Response.ClearHeaders();
        //    System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + "." + format);

        //    if (exportFormat == ExportFormat.Excel)
        //    {
        //        System.Web.HttpContext.Current.Response.ContentType = "application/excel";
        //    }
        //    else
        //    {
        //        System.Web.HttpContext.Current.Response.ContentType = format;
        //    }

        //    System.Web.HttpContext.Current.Response.BinaryWrite(fileContent);
        //    System.Web.HttpContext.Current.Response.End();


        //}

        internal static byte[] RenderContract(string contractName, Dictionary<string, string> parameters, string fileName)
        {
            byte[] fileContent = null;
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

                    fileContent = proxy.DownloadContract(contract, "HB", "0");
                }

                return fileContent;

            }
            catch (Exception ex)
            {

                System.Web.HttpContext.Current.Response.TrySkipIisCustomErrors = true;
                System.Web.HttpContext.Current.Response.StatusCode = 422;
                System.Web.HttpContext.Current.Response.StatusDescription = ex.Message;
                return fileContent;
            }


        }
    }
}