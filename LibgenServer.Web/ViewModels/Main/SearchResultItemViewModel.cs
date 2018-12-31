namespace LibgenServer.Web.ViewModels.Main
{
    public class SearchResultItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Series { get; set; }
        public string Year { get; set; }
        public string Publisher { get; set; }
        public string Format { get; set; }
        public string FileSize { get; set; }
        public bool Ocr { get; set; }
    }
}
