using ExternalBanking.ACBAServiceReference;
using ExternalBanking.PensionSystemRef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Կենսաթոշակային ֆոնդ
    /// </summary>
    public class PensionSystem
    {
        /// <summary>
        /// Կենսաթոշակային քաղվածքի մնացորդ
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// Գործուղության արդյունքի կոդ
        /// </summary>
        public ResultCode ResultCode { get; set; }

        /// <summary>
        /// Գործուղության արդյունքում առաջացած սխալներ
        /// </summary>
        public List<ActionError> Errors { get; set; }

        /// <summary>
        /// Անուն
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Անուն
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// ՀԾՀ
        /// </summary>
        public long PSN { get; set; }
        

        /// <summary>
        /// Վերադարձնում է կենսաթոշակային ֆոնդի մնացորդը
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <returns></returns>
        public void GetPensionBalance(ulong customerNumber)
        {
            Errors = new List<ActionError>();
            ACBAServiceReference.Customer customer;
            short customerType;
            using (ACBAOperationServiceClient proxy = new ACBAOperationServiceClient())
            {
                customer = (ACBAServiceReference.Customer)proxy.GetCustomer(customerNumber);
                customerType = customer.customerType.key;
                
            }

            if (customerType == (short)CustomerTypes.physical)
            {
                using (PensionSystemServiceClient pensionClient = new PensionSystemServiceClient())
                {
                    PersonData personData = new PersonData();

                    //Diana Start!!!
                    PhysicalCustomer physicalCustomer = customer as PhysicalCustomer;
                    DateTime? BirthDate = physicalCustomer.person.birthDate;

                    if (BirthDate != null)
                    {
                        personData.BirthDate = (DateTime)BirthDate;
                        personData.FirstName = physicalCustomer.person.fullName.firstNameEng;
                        personData.LastName = physicalCustomer.person.fullName.lastNameEng;

                        if (customer.residence.key == 1)
                        {
                            if (physicalCustomer.person.documentList.Exists(cd => cd.documentType.key == 56))
                            {
                                personData.PSN = Convert.ToInt64(Utility.ConvertAnsiToUnicode(physicalCustomer.person.documentList.Find(cd => cd.documentType.key == 56).documentNumber));
                            }
                            else if (physicalCustomer.person.documentList.Exists(cd => cd.documentType.key == 57))
                            {
                                personData.PSN = Convert.ToInt64(Utility.ConvertAnsiToUnicode(physicalCustomer.person.documentList.Find(cd => cd.documentType.key == 57).documentNumber));
                            }
                        }
                    }
                    //Diana End

                    //personData.BirthDate = DateTime.Parse("06/03/1974");
                    //personData.FirstName = "Clooney";
                    //personData.LastName = "George";
                    //personData.PSN = 7303830022;


                    var getBalanceResult = pensionClient.GetBalance(personData);

                    if (getBalanceResult.Item2.ResultCode == PensionSystemRef.ResultCode.Succeeded)
                    {
                        Balance = getBalanceResult.Item1;
                        this.BirthDate = personData.BirthDate;
                        FirstName = personData.FirstName;
                        LastName = personData.LastName;
                        PSN = personData.PSN;
                        ResultCode = ResultCode.Normal;
                    }
                    else
                    {
                        //Տվյալները գտնված չեն
                        Balance = null;
                        ResultCode = ResultCode.Normal;
                    }
                }       
            }
            else
            {
                //Կենսաթոշակային քաղվածքը նախատեսված է միայն ֆիզիկական անձ հաճախորդների համար։
                Errors.Add(new ActionError(1680));
                ResultCode = ResultCode.Failed;
            }
        }

    }
}
