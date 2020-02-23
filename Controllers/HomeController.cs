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
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Web.Helpers;

namespace JP2Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;

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
        public IActionResult UpdateUserInfo()
        {

            VEYMUser theUser = getVEYMUserGraphDataAsync().Result;

            // Pass the Goods to the View
            ViewData["XfirstName"] = theUser.FirstName;
            ViewData["XlastName"] = theUser.LastName;
            ViewData["Xrank"] = theUser.Rank;
            ViewData["Xleauge"] = theUser.Leauge;
            ViewData["Xchapter"] = theUser.Chapter;

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
                VEYMUser theUser = new VEYMUser();

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

            VEYMUser updatedVEYMUser = new VEYMUser();
            updatedVEYMUser.FirstName = Request.Query["FirstName"];
            updatedVEYMUser.LastName = Request.Query["LastName"];
            updatedVEYMUser.Rank = Request.Query["Rank"];
            updatedVEYMUser.Leauge = Request.Query["Leauge"];
            updatedVEYMUser.Chapter = Request.Query["Chapter"];

            VEYMUser oldVEYMUser = getVEYMUserGraphDataAsync().Result;

            // Pass the Goods to the View
            ViewData["oldFirstName"] = oldVEYMUser.FirstName;
            ViewData["oldLastName"] = oldVEYMUser.LastName;
            ViewData["oldRank"] = oldVEYMUser.Rank;
            ViewData["oldLeauge"] = oldVEYMUser.Leauge;
            ViewData["oldChapter"] = oldVEYMUser.Chapter;

            ViewData["updatedFirstName"] = updatedVEYMUser.FirstName;
            ViewData["updatedLastName"] = updatedVEYMUser.LastName;
            ViewData["updatedRank"] = updatedVEYMUser.Rank;
            ViewData["updatedLeauge"] = updatedVEYMUser.Leauge;
            ViewData["updatedChapter"] = updatedVEYMUser.Chapter;

            return View("Confirmation");
        }

        //Do Update here becasuse _graphSdkHelper already initalized
        public IActionResult Confirm(string firstName, string lastName, string rank, string leauge, string chapter)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                var identity = User.Identity as ClaimsIdentity;
                string email = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

                //Adding the next request is currentNumberOfRequests + 1
                WorkbookTableRow newRow = new WorkbookTableRow
                {
                    Values = JArray.Parse("[[\"" + email + "\",\"" + DateTime.Now + "\",\"" + firstName + "\",\"" + lastName +"\",\"" + rank + "\",\"" + leauge + "\",\"" + chapter + "\"]]")
                };

                //Add to the table!
                var outputResult = graphClient.Sites["veym.sharepoint.com,a1ece445-fd00-4466-a396-fd37d484cd87,4b350ed2-ce08-4bfd-a1c7-f7e1f327d840"].Drive.Items["01RHRJOHR5GDJRZ4IZFVC3W2BFQEME5OS5"].Workbook.Worksheets["mainsheet"].Tables["Table1"].Rows.Request().AddAsync(newRow).Result;

                // Pass the Goods to the View
                ViewData["XfirstName"] = firstName;
                ViewData["XlastName"] = lastName;
                ViewData["Xemail"] = email;
            }

            return View("ThankYou");
        }

        //Do Update here becasuse _graphSdkHelper already initalized
        public IActionResult Decline()
        {
            VEYMUser theUser = getVEYMUserGraphDataAsync().Result;

            // Pass the Goods to the View
            ViewData["XfirstName"] = theUser.FirstName;
            ViewData["XlastName"] = theUser.LastName;
            ViewData["Xrank"] = theUser.Rank;
            ViewData["Xleauge"] = theUser.Leauge;
            ViewData["Xchapter"] = theUser.Chapter;

            return View("UpdateUserInfo");
        }

        private async Task<VEYMUser> getVEYMUserGraphDataAsync()
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
            VEYMUser theUser = new VEYMUser();

            theUser.FirstName = currentUser.givenName;
            theUser.LastName = currentUser.surname;
            theUser.Rank = currentUser.rank;
            theUser.Leauge = currentUser.league;
            theUser.Chapter = currentUser.officeLocation;

            return theUser;
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
                VEYMUser theUser = new VEYMUser();
            }

            return View("FindUser");
        }

        public async Task<IActionResult> Resources()
        {
            return View();
        }
    }
}