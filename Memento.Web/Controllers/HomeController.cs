using Memento.Models.ViewModels;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Memento.Web.Controllers
{
#if !DEBUG
    [RequireHttps]
#endif
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Message = "Memento";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                if (storageAccount.FileEndpoint != null)
                {
                    var fileClient = storageAccount.CreateCloudFileClient();

                    var share = fileClient.GetShareReference("mementofileshare");

                    if (share.Exists())
                    {
                        var rootDir = share.GetRootDirectoryReference();

                        var docsDir = rootDir.GetDirectoryReference("docs");

                        if (docsDir.Exists())
                        {
                            var file = docsDir.GetFileReference("MementoDocs.html");

                            if (file.Exists())
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