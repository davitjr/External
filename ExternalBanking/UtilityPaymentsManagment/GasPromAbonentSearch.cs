using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.UtilityPaymentsManagment
{
    public class GasPromAbonentSearch
    {
        #region Properties

        /// <summary>
        /// Տեղամասի կոդը
        /// </summary>
        public string SectionCode { get; set; }

        /// <summary>
        /// Բաժանորդի քարտի համար
        /// </summary>
        public string AbonentNumber { get; set; }

        /// <summary>
        /// Բաժանորդի անուն
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Բաժանորդի ազգանուն
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Բաժանորդի հասցե
        /// </summary>

        public string Street { get; set; }

        /// <summary>
        /// Բաժանորդի տունը
        /// </summary>
        public string House { get; set; }
        /// <summary>
        /// Բաժանորդի բնակարանը
        /// </summary>

        public string Home { get; set; }

        /// <summary>
        /// Բաժանորդի հեռախոսահամար
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Պարտքացուցակի ամսաթիվ
        /// </summary>

        public DateTime DebtDate { get; set; }

        /// <summary>
        /// Պարտքը գազի դիմաց ամսվա սկզբին
        /// </summary>
        public double GasDebtAtBeginningOfMonth { get; set; }

        /// <summary>
        /// Նախորդ վճարում
        /// </summary>
        public double GasPreviousPayment { get; set; }

        /// <summary>
        /// Գազի ծախսը (խմ)
        /// </summary>
        public double GasExpenseByVolume { get; set; }

        /// <summary>
        /// Գազի ծախսը (դրամ)
        /// </summary>
        public double GasExpenseByAmount { get; set; }

        /// <summary>
        /// Պարտքը գազի դիմաց ամսվա վերջին
        /// </summary>
        public double DebtAtEndOfMonth { get; set; }

        /// <summary>
        /// Հաշվիչի նախորդ ցուցմունքը
        /// </summary>
        public double MeterPreviousTestimony { get; set; }

        /// <summary>
        /// Հաշվիչի վերջին ցուցմունք
        /// </summary>
        public double MeterLastTestimony { get; set; }

        /// <summary>
        /// Տույժ
        /// </summary>
        public double Penalty { get; set; }

        /// <summary>
        /// Խախտումը (խմ)
        /// </summary>
        public double ViolationByVolume { get; set; }

        /// <summary>
        /// Խախտումը (դրամ)
        /// </summary>
        public double ViolationByAmount { get; set; }

        /// <summary>
        /// Գազի վերահաշվարկ
        /// </summary>
        public double RecalculatedAmount { get; set; }

        /// <summary>
        /// Գազի սակագին
        /// </summary>
        public double Tariff { get; set; }

        /// <summary>
        /// Գազի ծախսը (խմ) նախորդ տարվա նույն ամսվա ընթացքում
        /// </summary>
        public double ExpenseByVolumeForSameMonthPreviousYear { get; set; }

        /// <summary>
        /// Պարտքը սպասարկման դիմաց ամսվա սկզբին
        /// </summary>
        public double ServiceFeeDebtAtBeginningOfMonth { get; set; }

        /// <summary>
        /// Սպասարկման նախորդ վճարում
        /// </summary>
        public double ServiceFeePreviousPayment { get; set; }

        /// <summary>
        /// Սպասարկման գումար (դրամ)
        /// </summary>
        public double ServiceFeeByAmount { get; set; }

        /// <summary>
        /// Պարտքը սպասարկման դիմաց ամսվա վերջին
        /// </summary>
        public double ServiceFeeAtEndOfMonth { get; set; }

        /// <summary>
        /// Սպասարկման վերահաշվարկ
        /// </summary>
        public double ServiceFeeRecalculatedAmount { get; set; }

        /// <summary>
        /// Վճարվել է գազի դիմաց ՝ ընթացիկ ամսվա մեջ
        /// </summary>
        public double PaidAmountInCurrentMonth { get; set; }

        /// <summary>
        /// Վճարվել է սպասարկման դիմաց՝ ընթացիկ ամսվա մեջ
        /// </summary>
        public double PaidServiceFeeInCurrentMonth { get; set; }

        /// <summary>
        /// Ընթացիկ պարտքը Գազի դիմաց 
        /// </summary>
        public double CurrentGasDebt { get; set; }

        /// <summary>
        /// Ընթացիկ պարտքը Գազի Սպասարկման դիմաց 
        /// </summary>
        public double CurrentServiceFeeDebt { get; set; }

        #endregion

        public static List<GasPromAbonentSearch> GasPromSearchOutput(SearchCommunal cmnl)
        {

            List<GasPromAbonentSearch> abonentSearch = new List<GasPromAbonentSearch>();
            try
            {
                GasPromSearchInput gasSearch = new GasPromSearchInput();
                GasPromSearchRequestResponse gasSearchRequest = new GasPromSearchRequestResponse();

                gasSearch.AbonentNumber = cmnl.AbonentNumber;
                gasSearch.SectionCode = cmnl.Branch;
                gasSearch.Name = cmnl.Name;
                gasSearch.LastName = cmnl.LastName;
                gasSearch.Street = cmnl.Street;
                gasSearch.House = cmnl.House;
                gasSearch.Home = cmnl.Home;
                    gasSearch.PhoneNumber = cmnl.PhoneNumber;

                gasSearchRequest = UtilityOperationService.GasPromAbonentSearch(gasSearch);

                foreach (var item in gasSearchRequest.GasPromSearchOutputList)
                {
                    GasPromAbonentSearch gasProm = new GasPromAbonentSearch();
                    gasProm = (GasPromAbonentSearch)item;

                    abonentSearch.Add(gasProm);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return abonentSearch;
        }

        public static explicit operator GasPromAbonentSearch(GasPromSearchOutput searchOutput)
        {
            GasPromAbonentSearch gasProm = new GasPromAbonentSearch();

            gasProm.SectionCode = searchOutput.SectionCode;
            gasProm.AbonentNumber = searchOutput.AbonentNumber;
            gasProm.Name = searchOutput.Name;
            gasProm.LastName = searchOutput.LastName;
            gasProm.Street = searchOutput.Street;
            gasProm.House = searchOutput.House;
            gasProm.Home = searchOutput.Home;
            gasProm.PhoneNumber = searchOutput.PhoneNumber;
            gasProm.DebtDate = searchOutput.DebtDate;
            gasProm.GasDebtAtBeginningOfMonth = searchOutput.GasDebtAtBeginningOfMonth;
            gasProm.GasPreviousPayment = searchOutput.GasPreviousPayment;
            gasProm.GasExpenseByVolume = searchOutput.GasExpenseByVolume;
            gasProm.GasExpenseByAmount = searchOutput.GasExpenseByAmount;
            gasProm.DebtAtEndOfMonth = searchOutput.DebtAtEndOfMonth;
            gasProm.MeterPreviousTestimony = searchOutput.MeterPreviousTestimony;
            gasProm.MeterLastTestimony = searchOutput.MeterLastTestimony;
            gasProm.Penalty = searchOutput.Penalty;
            gasProm.ViolationByVolume = searchOutput.ViolationByVolume;
            gasProm.ViolationByAmount = searchOutput.ViolationByAmount;
            gasProm.RecalculatedAmount = searchOutput.RecalculatedAmount;
            gasProm.Tariff = searchOutput.Tariff;
            gasProm.ExpenseByVolumeForSameMonthPreviousYear = searchOutput.ExpenseByVolumeForSameMonthPreviousYear;
            gasProm.ServiceFeeDebtAtBeginningOfMonth = searchOutput.ServiceFeeDebtAtBeginningOfMonth;
            gasProm.ServiceFeePreviousPayment = searchOutput.ServiceFeePreviousPayment;
            gasProm.ServiceFeeByAmount = searchOutput.ServiceFeeByAmount;
            gasProm.ServiceFeeAtEndOfMonth = searchOutput.ServiceFeeAtEndOfMonth;
            gasProm.ServiceFeeRecalculatedAmount = searchOutput.ServiceFeeRecalculatedAmount;
            gasProm.PaidAmountInCurrentMonth = searchOutput.PaidAmountInCurrentMonth;
            gasProm.PaidServiceFeeInCurrentMonth = searchOutput.PaidServiceFeeInCurrentMonth;
            gasProm.CurrentGasDebt = searchOutput.CurrentGasDebt;
            gasProm.CurrentServiceFeeDebt = searchOutput.CurrentServiceFeeDebt;

            return gasProm;
        }
    }
}
