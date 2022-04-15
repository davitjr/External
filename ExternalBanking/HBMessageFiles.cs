namespace ExternalBanking
{
    public class HBMessageFiles
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileContent { get; set; }
        public string RegistrationDate { get; set; }
    }
}
