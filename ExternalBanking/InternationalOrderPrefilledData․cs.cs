using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    public class InternationalOrderPrefilledData
    {
        #region Properties
        /// <summary>
        /// Անուն ազգանուն
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// Գրանցման երկիր
        /// </summary>
        public string SenderCountry { get; set; }
        /// <summary>
        /// Անձնագրային Տվյալներ
        /// </summary>
        public string SenderPassport { get; set; }
        /// <summary>
        /// Ծննդյան ամսաթիվ
        /// </summary>
        public DateTime SenderDateOfBirth { get; set; }
        /// <summary>
        /// Էլեկտրոնային հասցե
        /// </summary>
        public string SenderEmail { get; set; }
        /// <summary>
        /// Հեռախոսահամար
        /// </summary>
        public string SenderPhone { get; set; }
        /// <summary>
        /// ՀՎՀՀ
        /// </summary>
        public string SenderCodeOfTax { get; set; }

        #endregion

        #region Constructors
        public InternationalOrderPrefilledData()
        {

        }
        public InternationalOrderPrefilledData(ulong customernumber)
        {
            GetInternationalOrderPrefilledData(customernumber);
        }
        #endregion


        #region Methods
        public void GetInternationalOrderPrefilledData(ulong customerNumber)
        {
            ACBAServiceReference.Customer customerData = Customer.GetCustomer(customerNumber);

            List<CustomerAddress> addressList;

            //this.SenderType = customerData.customerType.key;

            if (customerData.customerType.key == 6)
            {
                Person person = ((customerData as PhysicalCustomer).person as Person);
                PhysicalCustomer physicalCustomer = customerData as PhysicalCustomer;
                this.SenderName = person.fullName.firstNameEng + " " + person.fullName.lastNameEng;
                CustomerDocument pass = person.documentList.Find(cd => cd.documentNumber == physicalCustomer.DefaultDocument && cd.defaultSign);
                this.SenderPassport = physicalCustomer.DefaultDocument + ", " + pass.givenBy + ", " + String.Format("{0:dd/MM/yy}", pass.givenDate);
                this.SenderDateOfBirth = person.birthDate != null ? (DateTime)person.birthDate : default(DateTime);
                this.SenderEmail = person.emailList != null && person.emailList.Count > 0 ? person.emailList[0].email.emailAddress : null;
                if (person.PhoneList.Count != 0)
                {
                    Phone phone;
                    if (person.PhoneList.FindAll(m => m.priority.key == 1).Count != 0)
                    {
                       phone = person.PhoneList.Find(m => m.priority.key == 1).phone;
                    }
                    else
                        phone = person.PhoneList.Find(m => m.priority.key == 0).phone;
                    this.SenderPhone = phone.countryCode + phone.areaCode + phone.phoneNumber;
                }
                addressList = person.addressList;
            }
            else
            {
                LegalCustomer legalCustomer = customerData as LegalCustomer;
                Organisation organisation = legalCustomer.Organisation;
                this.SenderName = legalCustomer.customerType.key == 2 ? "Private entrepreneur " + organisation.DescriptionEnglish : organisation.DescriptionEnglish;
                this.SenderCodeOfTax = Utility.ConvertAnsiToUnicode(legalCustomer.CodeOfTax);
                this.SenderEmail = organisation.emailList != null && organisation.emailList.Count > 0 ? organisation.emailList[0].email.emailAddress : null;
                if (organisation.phoneList.Count != 0)
                {
                    Phone phone;
                    if (organisation.phoneList.FindAll(m => m.priority.key == 1).Count != 0)
                    {
                        phone = organisation.phoneList.Find(m => m.priority.key == 1).phone;
                    }
                    else
                        phone = organisation.phoneList.Find(m => m.priority.key == 0).phone;
                    this.SenderPhone = phone.countryCode + phone.areaCode + phone.phoneNumber;
                }
                addressList = organisation.addressList;
            }
            CustomerAddress defaultAddress = addressList.Find(m => m.priority.key == 1);
            this.SenderCountry = defaultAddress.address.Country.key;

        } 
        #endregion
    }
}
