using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class SSTerminal
    {
        /// <summary>
        /// Տերմինալի ունիկալ համար
        /// </summary>
        public string TerminalID { get; set; }
        /// <summary>
        /// IP հասցե
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// Կայան
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Տերմինալի մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="terminal"></param>
        public SSTerminal(SSTerminal terminal)
        {
            this.TerminalID = terminal.TerminalID;
            this.HostName = terminal.HostName;
            this.IPAddress = terminal.IPAddress;
        }
        public SSTerminal() { }

        public static Account GetOperationSystemAccount(string terminalNumber, string currency)
        {
            return SSTerminalDB.GetOperationSystemAccount(terminalNumber, currency);
        }
        public static ushort GetTerminalFilial(string TerminalID)
        {
            return SSTerminalDB.GetTerminalFilial(TerminalID);
        }
        public static short CheckTerminalAuthorization(string terminalID, string ipAddress, string password)
        {
            short result;
            result = SSTerminalDB.CheckTerminalAuthorization(terminalID, ipAddress, password);
            return result;
        }
        public static Account GetOperationSystemTransitAccount(string terminalNumber, string currency)
        {
            return SSTerminalDB.GetOperationSystemTransitAccount(terminalNumber, currency);
        }
    }
}
