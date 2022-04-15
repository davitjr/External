using System.Collections.Generic;

namespace ExternalBanking
{
    public class BrokerContractSurvey
    {
        public BrokerContractSurvey()
        {
            StockKnowledges = new List<BrokerContractSelection>();
            RiskLeanings = new List<BrokerContractSelection>();
            FinancialExperiences = new List<BrokerContractSelection>();
            StockTools = new List<BrokerContractSelection>();
            FinancialSituations = new List<BrokerContractSelection>();
            Ocupations = new List<BrokerContractSelection>();
            Educations = new List<BrokerContractSelection>();
            StockExperiencDurations = new List<BrokerContractSelection>();
            InvestmentPurposes = new List<BrokerContractSelection>();
            BookValueOfPreviousYearOfAssets = new List<BrokerContractSelection>();
            LastYearSalesTurnovers = new List<BrokerContractSelection>();
            LastYearCapitals = new List<BrokerContractSelection>();
        }

        public List<BrokerContractSelection> StockKnowledges { get; set; }
        public List<BrokerContractSelection> RiskLeanings { get; set; }
        public List<BrokerContractSelection> FinancialExperiences { get; set; }
        public List<BrokerContractSelection> StockTools { get; set; }
        public List<BrokerContractSelection> StockExperiencDurations { get; set; }
        public List<BrokerContractSelection> FinancialSituations { get; set; }
        public List<BrokerContractSelection> Ocupations { get; set; }
        public List<BrokerContractSelection> Educations { get; set; }
        public List<BrokerContractSelection> InvestmentPurposes { get; set; }
        public List<BrokerContractSelection> BookValueOfPreviousYearOfAssets { get; set; }
        public List<BrokerContractSelection> LastYearSalesTurnovers { get; set; }
        public List<BrokerContractSelection> LastYearCapitals { get; set; }
    }

    public class BrokerContractSelection
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}