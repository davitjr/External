using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    /// <summary>
    /// Մ/Ճ-ի փաստաթղթի ստորագրող անձի փոփոխում
    /// </summary>
    public class DocumentSignatureSetting
    {
        /// <summary>
        /// Փոփոխություն կատարող օգտագործող
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Ստորագրող անձ(օր․՝ 1-Կառավարիչ,2-գլխ․հաշվապահ)
        /// </summary>
        public ushort SignatureType { get; set; }

        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime RegistartionDate { get; set; }

        /// <summary>
        ///  Մ/Ճ-ի փաստաթղթի ստորագրող անձի փոփոխման պահպանում
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveBranchDocumentSignatureSetting()
        {
            ActionResult result = new ActionResult();
            if (this.User.IsChiefAcc != true && this.User.IsManager != true)
            {
                //Տվյալ գործողությունը հասանելի չէ:
                result.Errors.Add(new ActionError(639));
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, new Culture(Languages.hy));
                return result;
            }
            result = DocumentSignatureSettingDB.SaveBranchDocumentSignatureSetting(this);
            
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Մ/Ճ-ի փաստաթղթի ստորագրող անձի կարգավորումը
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static DocumentSignatureSetting GetBranchDocumentSignatureSetting(ushort filialCode)
        {
            return DocumentSignatureSettingDB.GetBranchDocumentSignatureSetting(filialCode);
        }

    }
}
