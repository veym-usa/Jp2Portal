using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicrosoftGraphAspNetCoreConnectSample.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Linq;
using Jp2Portal.Models;
using Microsoft.SharePoint.Client;
using System;
using System.Text;
using System.Net;

namespace JP2Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;
        private VEYMUser theUser;

        public HomeController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphSdkHelper graphSdkHelper)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphSdkHelper = graphSdkHelper;
        }

        [AllowAnonymous]
        // Load user's profile.
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string email = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                string json = await GraphService.GetUserJson(graphClient, email, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);

                // Pass the Goods to the View
                ViewData["memberID"] = currentUser.memberID;
                ViewData["rank"] = currentUser.rank;
                ViewData["leauge"] = currentUser.league;
                ViewData["chapter"] = currentUser.officeLocation;
                ViewData["leaugeChapterID"] = leaugeChapterID;

                ViewData["Picture"] = await GraphService.GetPictureBase64(graphClient, email, HttpContext);
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> CampInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string email = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                string json = await GraphService.GetUserJson(graphClient, email, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);

                // Pass the Goods to the View
                ViewData["Xrank"] = currentUser.rank;
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult RegisterForEvent()
        {
            return View();
        }


        [AllowAnonymous]
        public async Task<IActionResult> UpdateUserInfo()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string email = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                string json = await GraphService.GetUserJson(graphClient, email, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);
                theUser = new VEYMUser();

                theUser.FirstName = currentUser.givenName;
                theUser.LastName = currentUser.surname;
                theUser.Rank = currentUser.rank;
                theUser.Leauge = currentUser.league;
                theUser.Chapter = currentUser.officeLocation;

                // Pass the Goods to the View
                ViewData["XfirstName"] = currentUser.givenName;
                ViewData["XlastName"] = currentUser.surname;
                ViewData["Xrank"] = currentUser.rank;
                ViewData["Xleauge"] = currentUser.league;
                ViewData["Xchapter"] = currentUser.officeLocation;
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> FindUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string email = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                string json = await GraphService.GetUserJson(graphClient, email, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);
                theUser = new VEYMUser();

                theUser.FirstName = currentUser.givenName;
                theUser.LastName = currentUser.surname;
                theUser.Rank = currentUser.rank;
                theUser.Leauge = currentUser.league;
                theUser.Chapter = currentUser.officeLocation;

                // Pass the Goods to the View
                ViewData["XfirstName"] = currentUser.givenName;
                ViewData["XlastName"] = currentUser.surname;
                ViewData["Xrank"] = currentUser.rank;
                ViewData["Xleauge"] = currentUser.league;
                ViewData["Xchapter"] = currentUser.officeLocation;
            }

            return View();
        }

        //Do Update here becasuse _graphSdkHelper already initalized
        public IActionResult ManageUpdate()
        {
            VEYMUser veymUser = new VEYMUser();
            veymUser.FirstName = Request.Query["FirstName"];
            veymUser.LastName = Request.Query["LastName"];
            veymUser.Rank = Request.Query["Rank"];
            veymUser.Leauge = Request.Query["Leauge"];
            veymUser.Chapter = Request.Query["Chapter"];

            string url = @"https://veym.sharepoint.com/:x:/s/ldtestdomain/camp-registration-DEV/ET0w0xzxGS1Fu2glgRhOul0BDq4hASmLE2tmB8_HQLekJw?e=GL7Zfc";

            using (var context = new ClientContext(new Uri(url)))
            {
                var web = context.Web;
                context.Credentials = new NetworkCredential();
                context.Load(web);
                try
                {
                    context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                }
                var file = web.GetFileByServerRelativeUrl(new Uri(url).AbsolutePath);
                context.Load(file);
                try
                {
                    context.ExecuteQuery();
                    file.SaveBinary(new FileSaveBinaryInformation() { Content = Encoding.UTF8.GetBytes("Hi.xls") });
                    try
                    {
                        context.ExecuteQuery();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
            }


            // Pass the Goods to the View
            ViewData["XfirstName"] = veymUser.FirstName;
            ViewData["XlastName"] = veymUser.LastName;
            ViewData["Xemail"] = User.FindFirst("preferred_username").Value;

            return View("ThankYou");
        }

        //Do a find
        public async Task<IActionResult> Find()
        {
            VEYMUser veymUser = new VEYMUser();
            veymUser.FirstName = Request.Query["FirstName"];
            veymUser.LastName = Request.Query["LastName"];
            veymUser.Rank = Request.Query["Rank"];
            veymUser.Leauge = Request.Query["Leauge"];
            veymUser.Chapter = Request.Query["Chapter"];

            string request = "https://graph.microsoft.com/v1.0/users?$filter=";

            if (!String.IsNullOrEmpty(veymUser.Rank))
            {
                request += "jobtitle eq " + veymUser.Rank;
            }

            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                 graphClient.BaseUrl = "https://graph.microsoft.com/beta/";

                string json = await GraphService.GetRequestJson(graphClient);
                
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);
                theUser = new VEYMUser();
            }




            return View("FindUser");
        }

        public async Task<IActionResult> Resources()
        {
            return View();
        }
    }
}