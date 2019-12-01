using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Memento.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;

namespace Neodenit.Memento.Web.Controllers
{
    public class HomeController : Controller
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        public async Task<ActionResult> Index()
        {
            ViewBag.Message = "Memento";

#if false
            try
            {
                var storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                if (storageAccount.FileEndpoint != null)
                {
                    var fileClient = storageAccount.CreateCloudFileClient();

                    var share = fileClient.GetShareReference("mementofileshare");

                    if (await share.ExistsAsync())
                    {
                        var rootDir = share.GetRootDirectoryReference();

                        var docsDir = rootDir.GetDirectoryReference("docs");

                        if (await docsDir.ExistsAsync())
                        {
                            var file = docsDir.GetFileReference("MementoDocs.html");

                            if (await file.ExistsAsync())
                            {
                                var text = await file.DownloadTextAsync();

                                return View(new HomeViewModel { HelpText = text });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
#endif

            return View(new HomeViewModel());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Memento";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Memento";

            return View();
        }
    }
}