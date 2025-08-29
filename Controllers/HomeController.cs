using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using SkiaSharp;

using Uno.Files.Options;
using Uno.Files.Options.Viewer;
using Uno.Files.Viewer;

using UnoViewer_Core_MVC.Models;

namespace UnoViewer_Core_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _accessor;

        public HomeController(IWebHostEnvironment hostingEnvironment, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _accessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewerSettings()
        {
            var viewerSettings = new ViewerSettings
            {
                LangFile = "en.json"
            };

            viewerSettings.PageSettings.PageMode = PageModeEnum.pan.ToString();

            viewerSettings.PageSettings.FitType = FitTypeEnum.width.ToString();
            viewerSettings.PageSettings.AutoFitPage = true;

            viewerSettings.PageSettings.AutoCopyText = true;
            viewerSettings.PageSettings.SelectTextColor = "gray";
            viewerSettings.PageSettings.CopyTextColor = "lime";

            viewerSettings.PageSettings.PageStatusLocation = PageStatusLocationEnum.bottom_right.ToString();
            viewerSettings.PageSettings.PageLayout = PageLayoutEnum.multiplePages.ToString();

            viewerSettings.ZoomSettings.PageZoom = 50;

            viewerSettings.SearchSettings.ActiveColor = "red";
            viewerSettings.SearchSettings.BorderStyle = "2px dashed black";
            viewerSettings.SearchSettings.BackColor = "lime";

            viewerSettings.ThumbSettings.ThumbImageQuality = 25;


            return Json(viewerSettings);
        }

        [HttpPost]
        public IActionResult OpenFile(string fileName)
        {
            var pathToFile = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, @"files\"), fileName);

            if (!System.IO.File.Exists(pathToFile))
            {
                Response.StatusCode = 404;
                return Content($"File does not exists: {pathToFile}");
            }

            var fileInfo = new FileInfo(pathToFile);

            var licenseFilePath = Path.Combine(Path.Combine(_hostingEnvironment.WebRootPath, "unoViewer"), "UnoViewer.xml.licx");

            var waterMark = new WaterMark
            {
                TextMark = "Hello World",
                Color = SKColors.Green,
                Font = new SKFont(SKTypeface.FromFamilyName("Verdana"), 30),
                Opacity = 20,
                Angle = -45,
                ShowOnCorners = true
            };

            var waterMarkString = UnoViewer.ApplyWatermark(waterMark);

            var viewOptions = new ViewOptions
            {
                Password = "",
                ImageResolution = 200,
                WatermarkInfo = waterMarkString,
                TimeOut = 30
            };

            var ctlUno = new UnoViewer(_cache, _accessor, licenseFilePath, viewOptions);


            BaseOptions? loadOptions = null;
            var pdfOptions = new PdfOptions { ExtractTexts = true, ExtractHyperlinks = true, AllowSearch = true };


            switch (fileInfo.Extension.ToUpper())
            {
                case ".DOC":
                case ".DOCX":
                case ".DOT":
                case ".DOTX":
                case ".ODT":
                case ".TXT":
                    loadOptions = new WordOptions { ConvertPdf = true, PdfOptions = pdfOptions, ImageResolution = 200 };
                    break;
                case ".XLS":
                case ".XLSX":
                case ".ODS":
                    loadOptions = new ExcelOptions { ExportOnePagePerWorkSheet = true, ShowEmptyWorkSheets = true, AutoFitContents = true, PdfOptions = pdfOptions };
                    break;
                case ".PPT":
                case ".PPS":
                case ".PPTX":
                case ".ODP":
                    loadOptions = new PptOptions { ConvertPdf = true, PdfOptions = pdfOptions };
                    break;

                case ".PDF":
                    loadOptions = pdfOptions;
                    break;
            }

            if (null != loadOptions)
            {
                try
                {
                    var token = ctlUno.ViewFile(pathToFile, loadOptions);

                    return Content(ctlUno.Token);

                }
                catch (Exception e)
                {
                    Response.StatusCode = 500;
                    return Content(e.Message);
                }
            }

            return Content("Error, Invalid file type");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
