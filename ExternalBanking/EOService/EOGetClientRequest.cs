using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExternalBanking
{
    public class EOGetClientRequest
    {
        /// <summary>
        /// Հարցման հերթական համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Ընկերության անվանում՝ "GoodCredit"
        /// </summary>
        public string CompID { get; set; }

        /// <summary>
        /// Անձնագրի/Նույնականացման քարտի համար
        /// </summary>
        public string Passport { get; set; }

        /// <summary>
        /// Սոց․ քարտի համար
        /// </summary>
        public long SSN { get; set; }

        /// <summary>
        /// Բանկային քարտի առաջին 6 և վերջին 6 նիշերը
        /// </summary>
        public string ProductID { get; set; }

        /// <summary>
        /// Բջջային հեռախոսի համար (երկրի կոդ + օպերատոր + համար)
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// Համագործակցող ընկերություն
        /// </summary>
        public CashTerminal Partner { get; set; }

        public SearchCustomers CreateDefaultSearchCustomer(SearchCustomers search)
        {

            search.quality = -1;
            search.residence = -1;
            search.customerType = -1;
            search.filialCode = -1;

            search.addressSearchParams = new Address();
            search.addressSearchParams.Country = new StringKeyValue();
            search.addressSearchParams.Region = new KeyValue();
            search.addressSearchParams.TownVillage = new KeyValue();
            search.addressSearchParams.Street = new IntKeyValue();
            search.addressSearchParams.Building = new KeyValue();
            return search;
        }

        public EOGetClientResponse GetClient()
        {



            EOServiceDB.SaveEOGetClientRequest(this);

            EOGetClientResponse response = new EOGetClientResponse();
            response.ParentID = this.ID;


            SearchCustomers search = new SearchCustomers();
            search = CreateDefaultSearchCustomer(search);



            if (this.SSN != 0)
            {
                search.socCardNumber = this.SSN.ToString();
            }

            if (this.Passport != "" && this.Passport != null)
            {
                search.passportNumber = this.Passport;
            }

            if (this.Telephone != "" && this.Telephone != null)
                search.phoneNumber = this.Telephone;

            if ((this.SSN == 0 || this.SSN.ToString().Length > 5) && ((this.Passport == "" || this.Passport == null) || this.Passport.Length > 5))
            {

                var searchResult = ACBAOperationService.FindCustomers(search, 1); // TODO paging

                List<SearchCustomers> customers = new List<SearchCustomers>();

                if (searchResult.First(m => m.Key >= 0).Key != 0)
                {
                    uint custmersCount = searchResult.First(m => m.Key >= 0).Key;
                    customers = searchResult[custmersCount];
                }

                customers = customers.FindAll(m => m.documentNumber == this.Passport || m.socCardNumber == this.SSN.ToString());
                if (customers.Count != 0)
                {
                    Account cardAccount = new Account();

                    foreach (SearchCustomers customer in customers)
                    {
                        if (!Validation.IsDAHKAvailability(Convert.ToUInt64(customer.customerNumber)))
                        {
                            List<Card> customerCardsList = Card.GetCards(Convert.ToUInt64(customer.customerNumber), ProductQualityFilter.Opened);
                            foreach (Card card in customerCardsList)
                            {
                                if (this.ProductID == card.CardNumber.Substring(0, 8) + card.CardNumber.Substring(card.CardNumber.Length - 4))
                                {
                                    if (card.Currency == "AMD")
                                    {
                                        response.Account = Convert.ToInt64(card.CardAccount.AccountNumber);
                                        response.ErrorCode = 0;
                                        response.ErrorText = "";
                                    }
                                    else
                                    {
                                        response.Account = Convert.ToInt64(card.CardAccount.AccountNumber);
                                        response.ErrorCode = 1;
                                        response.ErrorText = "Foreign currency card";
                                    }
                                    break;
                                }

                            }
                            if (response.Account == 0)
                            {
                                response.Account = 0;
                                response.ErrorCode = 1;
                                response.ErrorText = "Product not found";
                            }
                        }
                        else
                        {
                            response.Account = 0;
                            response.ErrorCode = 1;
                            response.ErrorText = "Found in stop list";
                        }

                    }
                }

                else
                {
                    response.Account = 0;
                    response.ErrorCode = 1;
                    response.ErrorText = "Client not found";
                }
            }
            else
            {
                response.Account = 0;
                response.ErrorCode = 1;
                response.ErrorText = "Client not found";
            }
            response.ID = EOServiceDB.SaveEOGetClientResponse(response);
            return response;
        }
    }
}

