namespace ExternalBanking
{

    public class Culture
    {
        internal Languages Language { get; set; }

        //internal bool IsUnicode { get; set;}

        //public Culture()
        //{
        //	IsUnicode = true;
        //}

        public Culture(Languages language) //:base()
        {
            Language = language;
        }

    }

}

