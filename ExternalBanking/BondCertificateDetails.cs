using System;

public class BondCertificateDetails
{
    public string ISIN { get; set; }

    /// <summary>
    /// Ձեռք բերվող պարտատոմսերի քանակ
    /// </summary>
    public int BondCount { get; set; }

    /// <summary>
    /// Գրանցման օր
    /// </summary>
    public DateTime RegistrationDate { get; set; }
    
    public string FullName { get; set; }

    public int ClientType { get; set; }

}
