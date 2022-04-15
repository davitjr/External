namespace ExternalBanking
{
    class ActionResultToken : ActionResult
    {
        public int ErrorResponsecode { get; set; }
        public string Description { get; set; }
    }
}
