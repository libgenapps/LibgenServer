using System;
using LibgenServer.Web.Models;
using LibgenServer.Web.ViewModels.Main;
using Microsoft.AspNetCore.Mvc;

namespace LibgenServer.Web.Controllers
{
    public class MainController : Controller
    {
        private readonly MainModel mainModel;

        public MainController(MainModel mainModel)
        {
            this.mainModel = mainModel;
        }

        [Route("~/")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Route("~/")]
        [HttpPost]
        public IActionResult Index(string search)
        {
            if (String.IsNullOrWhiteSpace(search))
            {
                return View();
            }
            else
            {
                return RedirectToAction(nameof(Search), new { searchQuery = search });
            }
        }

        [Route("Search/{*searchQuery}")]
        public IActionResult Search(string searchQuery)
        {
            SearchViewModel searchViewModel = new SearchViewModel()
            {
                SearchQuery = searchQuery
            };
            searchViewModel.SearchResults = mainModel.Search(searchQuery);
            return View(searchViewModel);
        }

        [Route("Book/{bookId}")]
        public IActionResult Book(int bookId)
        {
            BookViewModel bookViewModel = mainModel.GetBookViewModel(bookId);
            return View(bookViewModel);
        }

        [Route("Download/{md5Hash:length(32)}")]
        public IActionResult DownloadBook(string md5Hash)
        {
            DownloadBookViewModel downloadBookViewModel = mainModel.GetDownloadBookViewModel(md5Hash);
            if (downloadBookViewModel == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return PhysicalFile(downloadBookViewModel.LocalFilePath, downloadBookViewModel.ContentType, downloadBookViewModel.DownloadFileName, true);
        }
    }
}