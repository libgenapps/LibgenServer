using System.Collections.Generic;

namespace LibgenServer.Web.ViewModels.Main
{
    public class SearchViewModel
    {
        public string SearchQuery { get; set; }
        public List<SearchResultItemViewModel> SearchResults { get; set; }
    }
}
