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

namespace MicrosoftGraphAspNetCoreConnectSample.Controllers
{
    public class UpdateUserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;

        public UpdateUserController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphSdkHelper graphSdkHelper)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphSdkHelper = graphSdkHelper;
        }

        [AllowAnonymous]
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
    }
}