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

namespace MicrosoftGraphAspNetCoreConnectSample.Controllers
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
        public IActionResult CampInfo()
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

        //Do Update here becasuse _graphSdkHelper already initalized
        public async Task<IActionResult> Update()
        {
            theUser = new VEYMUser();
            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";

                
                var user = new User
                {
                    
                };

                //Do the Update
                //await graphClient.Me.Request().UpdateAsync(user);

            }

            return View();
        }
    }
}