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
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VEYMService.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using VEYMServices.Models;
using User = VEYMServices.Models.User;

namespace JP2Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly IGraphSdkHelper _graphSdkHelper;
        private Jp2Portal.Helpers.VEYMService theVEYMService;

        public HomeController(IConfiguration configuration, IHostingEnvironment hostingEnvironment, IGraphSdkHelper graphSdkHelper)
        {
            _configuration = configuration;
            _env = hostingEnvironment;
            _graphSdkHelper = graphSdkHelper;
            theVEYMService = new Jp2Portal.Helpers.VEYMService();
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

                //See if User is in the admins Table

                //Call VEYM Service, Open Info and iterate through the admins
                //if the user is in admins

                List<string> listOfAdminEmailAddresses = await getAdminListHelper();

                bool userIsAdmin = listOfAdminEmailAddresses.Contains(email);

                //Register the User if they are not in the Users Table
                if (!(await isUserInSystemAsync(email)))
                {
                    await addUserToSystemHelperAsync(email);
                }

                ViewData["Admin"] = userIsAdmin;
                ViewData["MembershipID"] = await getMembershipIDHelper(email);
                ViewData["Xrank"] = currentUser.rank;
            }

            return View();
        }

        private async Task<bool> isUserInSystemAsync(string email)
        {
            List<User> allUsersInSystem = await theVEYMService.getAllUserAsync();

            foreach (User user in allUsersInSystem)
            {
                if (user.emailAddress == email)
                {
                    return true;
                }
            }

            return false;
        }

        [AllowAnonymous]
        public IActionResult RegisterForEvent()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AdminMode()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> FindMembershipID()
        {
            String emailToSeachFor = Request.Query["Email"];

            // Initialize the GraphServiceClient.
            var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
            graphClient.BaseUrl = "https://graph.microsoft.com/beta/";

            try
            {
                string memberID = await getMembershipIDHelper(emailToSeachFor);

                if (memberID != null)
                {
                    // Pass the Goods to the View
                    ViewData["OutputTextPrompt"] = "Find Success!!! MembershipID for " + emailToSeachFor + " is ";
                    ViewData["OutputTextResult"] = memberID;
                }

            }
            catch
            {
                if (emailToSeachFor != null)
                {
                    ViewData["OutputTextResult"] = "Error in Find request: " + "Unable to find membershipID for email addresss " + emailToSeachFor;
                }
            }

            return View("AdminTools");
        }

        private async Task<string> getMembershipIDHelper(string email)
        {
            // Initialize the GraphServiceClient.
            var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
            graphClient.BaseUrl = "https://graph.microsoft.com/beta/";

            try
            {
                var user = await graphClient.Users[email].Request().GetAsync();

                user.AdditionalData.TryGetValue("extension_4d982a3099ee47359aed5ac368c6d277_MemberID_Legacy", out var memberID);

                if (memberID != null)
                {
                    string returnValue = memberID.ToString();
                    return returnValue;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                return "";
            }
        }

        private async Task<List<string>> getAdminListHelper()
        {
            Info Info = await theVEYMService.getInfoAsync();

            return Info.listOfAdminEmailAddresses;
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignUpUserToTraining()
        {
            String membershipID = Request.Query["MembershipIDR"];
            String traininingID = Request.Query["TrainingIDR"];

            SignUp signupRequest = new SignUp();
            signupRequest.membershipID = membershipID;
            signupRequest.trainingID = traininingID;

            string jsonRequestBody = JsonConvert.SerializeObject(signupRequest);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var response = await client.PutAsync("https://localhost:44304/api/training", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseData = await response.Content.ReadAsAsync<String>();

                // Pass the Goods to the View
                ViewData["OutputTextPrompt"] = "Sign Up Success!!! Registration for MembershipID " + membershipID + " for Training " + traininingID + " has been processed! ";
                ViewData["OutputTextResult"] = "They are priority" + responseData.Substring(responseData.LastIndexOf(':'));
            }

            return View("AdminTools");
        }

        [AllowAnonymous]
        public async Task<IActionResult> UnprioritizeUserInTraining()
        {
            String membershipID = Request.Query["MembershipIDU"];
            String traininingID = Request.Query["TrainingIDU"];

            Unregister unprioritize = new Unregister();
            unprioritize.membershipID = membershipID;
            unprioritize.trainingID = traininingID;
            unprioritize.sendToTheBack = true;

            string jsonRequestBody = JsonConvert.SerializeObject(unprioritize);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var deleteRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://localhost:44304/api/training"),
                Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(deleteRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseData = await response.Content.ReadAsAsync<String>();

                // Pass the Goods to the View
                ViewData["OutputTextPrompt"] = "Unprioritization Success!!! Unprioritization for MembershipID " + membershipID + " for " + traininingID + "has been processed! ";
                ViewData["OutputTextResult"] = "They are priority " + responseData.Substring(responseData.LastIndexOf(':'));
            }

            return View("AdminTools");
        }

        [AllowAnonymous]
        public async Task<IActionResult> CreateTraining()
        {
            String trainingName = Request.Query["TrainingName"];
            String trainingUserCapacity = Request.Query["TrainingUserCapacity"];

            Training training = new Training();
            training.trainingName = trainingName;
            training.trainingUserCapacity = int.Parse(trainingUserCapacity);

            string jsonRequestBody = JsonConvert.SerializeObject(training);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var response = await client.PostAsync("https://localhost:44304/api/training", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseData = await response.Content.ReadAsAsync<String>();

                // Pass the Goods to the View
                ViewData["OutputTextPrompt"] = "Create Success!!! Training " + trainingName + " has been created with capacity " + trainingUserCapacity;
            }

            return View("AdminTools");
        }

        [AllowAnonymous]
        public async Task<IActionResult> DropUserInTraining()
        {
            String membershipID = Request.Query["MembershipIDD"];
            String traininingID = Request.Query["TrainingIDD"];

            Unregister unprioritize = new Unregister();
            unprioritize.membershipID = membershipID;
            unprioritize.trainingID = traininingID;
            unprioritize.sendToTheBack = false;

            string jsonRequestBody = JsonConvert.SerializeObject(unprioritize);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var deleteRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://localhost:44304/api/training"),
                Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(deleteRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseData = await response.Content.ReadAsAsync<String>();

                // Pass the Goods to the View
                ViewData["OutputTextPrompt"] = "Drop Success!!! MembershipID " + membershipID + " for Training " + traininingID + " has been dropped! ";
            }

            return View("AdminTools");
        }

        [AllowAnonymous]
        public async Task<IActionResult> RegisterUserToSystem()
        {
            String membershipEmail = Request.Query["EmailR"];

            try
            {
                VEYMServices.Models.User registerRequest = new VEYMServices.Models.User();

                //popualte registerRequest with Graph Data

                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data for the requestedEmail
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string json = await GraphService.GetUserJson(graphClient, membershipEmail, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);

                // Set up the request from the Beta Data
                registerRequest.membershipID = currentUser.memberID;
                registerRequest.emailAddress = membershipEmail;
                registerRequest.name = currentUser.displayName;
                registerRequest.rank = currentUser.rank;
                registerRequest.chapter = currentUser.officeLocation;
                registerRequest.leaugeOfChapters = currentUser.league;

                string jsonRequestBody = JsonConvert.SerializeObject(registerRequest);

                //prep to send to the VEYMService
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:44304");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

                var response = await client.PostAsync("https://localhost:44304/api/users", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    // Pass the Goods to the View
                    ViewData["OutputTextPrompt"] = "Registration Success!!! Registration for " + membershipEmail + " fully processed!";
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Pass the Goods to the View
                    ViewData["OutputTextPrompt"] = "Registration Failure!!! Registration for " + membershipEmail + " was unsucessfull!";
                }
            }
            catch (Exception e)
            {
                // Pass the Goods to the View
                ViewData["OutputTextPrompt"] = "Registration Failure!!! Registration for " + membershipEmail + " was unsucessfull!";
                return View("AdminMode");
            }

            return View("AdminTools");
        }

        private async Task<bool> addUserToSystemHelperAsync(string membershipEmail)
        {
            try
            {
                VEYMServices.Models.User registerRequest = new VEYMServices.Models.User();

                //popualte registerRequest with Graph Data

                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data for the requestedEmail
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";
                var identity = User.Identity as ClaimsIdentity;
                string json = await GraphService.GetUserJson(graphClient, membershipEmail, HttpContext);
                UserDataObjectBETA.RootObject currentUser = JsonConvert.DeserializeObject<UserDataObjectBETA.RootObject>(json);
                string leaugeChapterID = currentUser?.chapter?.Substring(currentUser.chapter.IndexOf(';') + 1);

                // Set up the request from the Beta Data
                registerRequest.membershipID = currentUser.memberID;
                registerRequest.emailAddress = membershipEmail;
                registerRequest.name = currentUser.displayName;
                registerRequest.rank = currentUser.rank;
                registerRequest.chapter = currentUser.officeLocation;
                registerRequest.leaugeOfChapters = currentUser.league;

                string jsonRequestBody = JsonConvert.SerializeObject(registerRequest);

                //prep to send to the VEYMService
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:44304");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

                var response = await client.PostAsync("https://localhost:44304/api/users", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
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
        public async Task<IActionResult> Confirm(string firstName, string lastName, string rank, string leauge, string chapter)
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
                    Values = JArray.Parse("[[\"" + email + "\",\"" + DateTime.Now + "\",\"" + firstName + "\",\"" + lastName + "\",\"" + rank + "\",\"" + leauge + "\",\"" + chapter + "\"]]")
                };

                //Add to the table!
                var outputResult = graphClient.Sites["veym.sharepoint.com,a1ece445-fd00-4466-a396-fd37d484cd87,4b350ed2-ce08-4bfd-a1c7-f7e1f327d840"].Drive.Items["01RHRJOHR5GDJRZ4IZFVC3W2BFQEME5OS5"].Workbook.Worksheets["mainsheet"].Tables["Table1"].Rows.Request().AddAsync(newRow).Result;

                //Send Email
                var message = new Message
                {
                    Subject = "User Data Update Request!",
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = "Please update my User Data with this body: \n\n" +
                        "PATCH https://graph.microsoft.com/v1.0/users/" + email + " \nContent - type: application /\n\n json\n" +
                        "{" +
                        "\"extension_4d982a3099ee47359aed5ac368c6d277_Chapter\": \"" + chapter + "\"," +
                        "\"extension_4d982a3099ee47359aed5ac368c6d277_League\": \"" + leauge + "\"," +
                        "\"extension_4d982a3099ee47359aed5ac368c6d277_Rank\": \"" + rank + "\"," +
                        "\"givenName\": \"" + firstName + "\"," +
                        "\"surname\": \"" + lastName + "\"" +
                        "}"
                    },
                    ToRecipients = new List<Recipient>()
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = "philips.nguyen@veym.net"
                            }
                        }
                    },
                    CcRecipients = new List<Recipient>()
                    {

                    }
                };

                await graphClient.Me
                .SendMail(message, false)
                .Request()
                .PostAsync();

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
            VEYMUser userFindCriteria = new VEYMUser();
            userFindCriteria.FirstName = Request.Query["FirstName"];
            userFindCriteria.LastName = Request.Query["LastName"];
            userFindCriteria.Rank = Request.Query["Rank"];
            userFindCriteria.Leauge = Request.Query["Leauge"];
            userFindCriteria.Chapter = Request.Query["Chapter"];
            int counter = 0;
            string informationalText = "Users with: ";

            if (User.Identity.IsAuthenticated)
            {
                // Initialize the GraphServiceClient.
                var graphClient = _graphSdkHelper.GetAuthenticatedClient((ClaimsIdentity)User.Identity);
                // Grab Beta Data
                graphClient.BaseUrl = "https://graph.microsoft.com/beta/";

                IGraphServiceUsersCollectionRequest requestBase = graphClient.Users.Request();
                string buildpart = "";

                if (userFindCriteria.FirstName != "?")
                {
                    buildpart += "startswith(givenName, '" + userFindCriteria.FirstName + "') and ";
                    informationalText += " First Name starting with ->" + userFindCriteria.FirstName + ";";
                }

                if (userFindCriteria.LastName != "?")
                {
                    buildpart += "startswith(surname, '" + userFindCriteria.LastName + "') and ";
                    informationalText += " Last Name starting with ->" + userFindCriteria.LastName + ";";
                }

                if (userFindCriteria.Rank != "?")
                {
                    buildpart += "startswith(jobTitle, '" + userFindCriteria.Rank + "') and ";
                    informationalText += " Who have obtained the rank ->" + userFindCriteria.Rank + ";";
                }

                if (userFindCriteria.Leauge != "?")
                {
                    buildpart += "startswith(extension_4d982a3099ee47359aed5ac368c6d277_League, '" + userFindCriteria.Leauge + "') and ";
                    informationalText += " Who are in the Lien Doan ->" + userFindCriteria.Leauge + ";";
                }

                if (userFindCriteria.Chapter != "?")
                {
                    buildpart += "startswith(officeLocation, '" + userFindCriteria.Chapter + "') and ";
                    informationalText += " Who are in the Doan ->" + userFindCriteria.Chapter + ";";
                }

                buildpart = buildpart.Substring(0, buildpart.Length - 4);

                var userReturnObject = await requestBase.Filter(buildpart).GetAsync();

                foreach (Microsoft.Graph.User user in userReturnObject.CurrentPage)
                {
                    ViewData["userName" + counter] = user.DisplayName;
                    ViewData["userEmail" + counter] = user.Mail;
                    ViewData["userRank" + counter] = user.JobTitle;
                    ViewData["userChapter" + counter] = user.OfficeLocation;

                    if (user.AdditionalData.ContainsKey("extension_4d982a3099ee47359aed5ac368c6d277_League"))
                    {
                        ViewData["userLeauge" + counter] = user.AdditionalData["extension_4d982a3099ee47359aed5ac368c6d277_League"];
                    }
                    else
                    {
                        ViewData["userLeauge" + counter] = "No Data Available";
                    }

                    ViewData["userPicture" + counter] = await GraphService.GetPictureBase64(graphClient, user.Mail, HttpContext);

                    counter++;
                }

                ViewData["maxUsersToDisplay"] = counter;
            }

            ViewData["searchInformationalText"] = informationalText;
            ViewData["numberOfUsersReturned"] = counter;

            return View("FindUser");
        }

        public async Task<IActionResult> Resources()
        {
            return View();
        }

        public async Task<IActionResult> AdminTools()
        {
            return View();
        }

        public async Task<IActionResult> AdministrateTrainings()
        {
            ViewData["TrainingToShow"] = Request.Query["TrainingToShow"];

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:44304/api/training"),
                Content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            List<Training> responseData = await response.Content.ReadAsAsync<List<Training>>();

            ViewData["numberOfTrainings"] = responseData.Count;

            int counter = 0;

            foreach (Training training in responseData)
            {
                ViewData["trainingName" + counter] = training.trainingName + " (" + training.trainingID + ")";
                counter++;
            }

            return View();
        }

        public async Task<IActionResult> DisplayTraining()
        {
            String training = Request.Query["training"];
            string trainingID = training.Substring(training.LastIndexOf("(") + 1).Replace(")", "");

            Training trainingRequest = new Training();
            trainingRequest.trainingID = trainingID;

            string jsonRequestBody = JsonConvert.SerializeObject(trainingRequest);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:44304/api/training"),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            Training responseData = await response.Content.ReadAsAsync<Training>();

            int counter = 0;

            ViewData["trainingID"] = responseData.trainingID;
            ViewData["trainingName"] = responseData.trainingName;
            ViewData["numberOfUsers"] = responseData.signupList.Count;

            foreach (TrainingUserItem trainingUserItem in responseData.signupList)
            {
                //Populate the Chart
                ViewData["membershipID" + counter] = trainingUserItem.membershipID;
                ViewData["placeInQueue" + counter] = trainingUserItem.placeInQueue;
                ViewData["timeSignedUp" + counter] = trainingUserItem.timeSignedUp;

                //Query Users For User Data

                User user = theVEYMService.getUserAsync(trainingUserItem.membershipID).Result;

                //Name
                ViewData["name" + counter] = user.name;
                //Email Addresss
                ViewData["emailAddress" + counter] = user.emailAddress;

                counter++;
            }

            return View();
        }

        public async Task<IActionResult> UserTrainings()
        {
            string fullPath = Request.Path.Value;
            string firstPartInput = fullPath.Substring(fullPath.LastIndexOf("/") + 1);
            string membershipID;

            // PLauged by the controller bugamazon
            if (firstPartInput.Contains("ReturnToAdministrateUsers"))
            {
                return await AdministrateUsers();
            }

            // Bug workaround since "Unprioritize" is defaulting to this controller
            if (firstPartInput[0] == 'U')
            {
                string[] moreParts = firstPartInput.Split('X');
                membershipID = moreParts[0].Substring(1);
                string trainingID = moreParts[1];

                ViewData["DisplayMessage"] = await theVEYMService.deprioritizeUserInTrainingAsync(trainingID, membershipID);

            }
            // Bug workaround since "Delete" is defaulting to this controller
            else if (firstPartInput[0] == 'D')
            {
                string[] moreParts = firstPartInput.Split('X');
                membershipID = moreParts[0].Substring(1);
                string trainingID = moreParts[1];

                ViewData["DisplayMessage"] = await theVEYMService.dropUserInTrainingAsync(trainingID, membershipID);
            }
            else
            {
                membershipID = firstPartInput;
            }

            VEYMServices.Models.User user = new User();
            user.membershipID = membershipID;

            string jsonRequestBody = JsonConvert.SerializeObject(user);

            //prep to send to the VEYMService
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44304");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:44304/api/users"),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            User responseData = await response.Content.ReadAsAsync<User>();

            List<string> trainingCamps = responseData.trainingCamps;

            ViewData["Name"] = responseData.name;

            ViewData["numberOfTrainings"] = trainingCamps.Count;

            foreach (String trainingID in trainingCamps)
            {
                //Query Training based on TrainingID

                Training training = new Training();
                training.trainingID = trainingID;

                jsonRequestBody = JsonConvert.SerializeObject(training);

                getRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://localhost:44304/api/training"),
                    Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                };

                response = await client.SendAsync(getRequest);

                Training trainingData = await response.Content.ReadAsAsync<Training>();

                List<TrainingUserItem> trainingSignUpList = trainingData.signupList;

                int counter = 0;

                foreach (TrainingUserItem userItem in trainingSignUpList)
                {
                    if (userItem.membershipID == membershipID)
                    {
                        //Display this
                        ViewData["trainingID" + counter] = trainingData.trainingID;
                        ViewData["trainingName" + counter] = trainingData.trainingName;
                        ViewData["status" + counter] = "Awaiting Payment";
                        ViewData["placeInQueue" + counter] = userItem.placeInQueue;
                        ViewData["timeSignedUp" + counter] = userItem.timeSignedUp;
                        ViewData["Paid" + counter] = "No";
                        ViewData["membershipID" + counter] = membershipID;

                        counter++;
                    }
                }
            }

            return View();

        }

        public async Task<IActionResult> AdministrateUsers()
        {
            string fullPath = Request.Path.Value;
            try
            {
                //prep to send to the VEYMService
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:44304");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", theVEYMService.getBearerToken());

                var getRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://localhost:44304/api/users"),
                    Content = new StringContent("")
                };

                var response = await client.SendAsync(getRequest);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //Read all users
                    string responseData = await response.Content.ReadAsStringAsync();
                    List<User> users = JsonConvert.DeserializeObject<List<User>>(responseData);

                    ViewData["numberOfUsers"] = users.Count;

                    int counter = 0;
                    foreach (User user in users)
                    {
                        ViewData["membershipID" + counter] = user.membershipID;
                        ViewData["emailAddress" + counter] = user.emailAddress;
                        ViewData["name" + counter] = user.name;
                        counter++;
                    }

                    return View("AdministrateUsers");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Pass the Goods to the View
                }
            }
            catch (Exception e)
            {
                // Pass the Goods to the View
                return View("AdminMode");
            }

            return View("AdministrateUsers");
        }

        public async Task<IActionResult> TrainingDropUserFromTraining()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string[] splitByX = fullPathInParts[fullPathInParts.Length - 1].Split("X");
            string membershipID = splitByX[0];
            string trainingID = splitByX[1];

            //dropUserInTrainingAsync(membershipID);

            return View("DisplayTraining");
        }

        public async Task<IActionResult> DeleteTraining()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string[] splitByX = fullPathInParts[fullPathInParts.Length - 1].Split("X");
            string trainingID = splitByX[1];

            return View("DisplayTraining");
        }

        public async Task<IActionResult> userDropUserFromTraining()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string[] splitByX = fullPathInParts[fullPathInParts.Length - 1].Split("X");
            string membershipID = splitByX[0];
            string trainingID = splitByX[1];

            string response = await theVEYMService.dropUserInTrainingAsync(trainingID, membershipID);

            return View("DisplayTraining");
        }

        public async Task<IActionResult> userDeleteTraining()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string[] splitByX = fullPathInParts[fullPathInParts.Length - 1].Split("X");
            string trainingID = splitByX[1];

            return View("DisplayTraining");
        }

        public async Task<IActionResult> deleteUser()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string membershipID = fullPathInParts[fullPathInParts.Length - 1];

            await theVEYMService.deleteUserAsync(membershipID);

            return View("DisplayTraining");
        }

        public async Task<IActionResult> PreviousTrainings()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string inputParam = fullPathInParts[fullPathInParts.Length - 1];

            //Plauged by the weirdController Bug
            if (inputParam.Contains("T"))
            {
                //Query And show All users from that camp
                string trainingID = inputParam.Replace("T", "");

                //Query for training
                Training training = await theVEYMService.getTrainingAsync(trainingID);

                ViewData["NumberOfUsers"] = training.signupList.Count;

                int counter = 0;
                foreach (TrainingUserItem trainingUserItem in training.signupList)
                {
                    //Query for the User
                    User user = await theVEYMService.getUserAsync(trainingUserItem.membershipID);

                    ViewData["UserName" + counter] = user.name;
                    ViewData["UserEmail" + counter] = user.emailAddress;
                    ViewData["UserChapter" + counter] = user.chapter;
                    ViewData["UserLeaugeOfChapter" + counter] = user.leaugeOfChapters;
                    counter++;
                }

                ViewData["TrainingName"] = training.trainingName;

                return View("ViewTraining");
            }
            else
            {
                string membershipID = inputParam;
                User user = await theVEYMService.getUserAsync(membershipID);

                ViewData["numberOfTrainings"] = user.trainingCamps.Count;

                int counter = 0;
                foreach (string trainingID in user.trainingCamps)
                {
                    ViewData["trainingID" + counter] = trainingID;

                    //Query for training
                    Training training = await theVEYMService.getTrainingAsync(trainingID);

                    ViewData["trainingName" + counter] = training.trainingName;

                    //Look for the User in the training\

                    foreach (TrainingUserItem trainingUserItem in training.signupList)
                    {
                        if (trainingUserItem.membershipID == membershipID)
                        {
                            ViewData["trainingUserStatus" + counter] = trainingUserItem.status;
                        }
                    }

                    counter++;
                }

                // Select The User by Membership ID
                // Iterate through the number of trainings
                return View();
            }
        }
        public async Task<IActionResult> ViewTraining()
        {
            //Query And show All users from that camp
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string trainingID = fullPathInParts[fullPathInParts.Length - 1];

            //Query for training
            Training training = await theVEYMService.getTrainingAsync(trainingID);

            ViewData["NumberOfUsers"] = training.signupList.Count;

            int counter = 0;
            foreach (TrainingUserItem trainingUserItem in training.signupList)
            {
                //Query for the User
                User user = await theVEYMService.getUserAsync(trainingUserItem.membershipID);

                ViewData["UserName" + counter] = user.name;
                ViewData["UserEmail" + counter] = user.emailAddress;
                ViewData["UserChapter" + counter] = user.chapter;
                ViewData["UserLeaugeOfChapter" + counter] = user.leaugeOfChapters;
                counter++;
            }


            return await ViewTraining();
        }

        //Bug with multiple drops
        public async Task<IActionResult> CurrentRegistrations()
        {
            string[] fullPathInParts = Request.Path.Value.Split("/");
            string inputPart = fullPathInParts[fullPathInParts.Length - 1];
            string membershipID = "";
            string trainingIDDrop = "";

            if (inputPart.Contains("D"))
            {
                string[] splitByX = inputPart.Split("X");
                trainingIDDrop = splitByX[0].Replace("D","");
                membershipID = splitByX[1];

                string response = await theVEYMService.dropUserInTrainingAsync(trainingIDDrop, membershipID);

                if (response.Contains("unregistered"))
                {
                    Training training = await theVEYMService.getTrainingAsync(trainingIDDrop);
                    ViewData["Message"] = "You have dropped from " + training.trainingName;
                }


            }
            else
            {
                membershipID = inputPart;
            }
             
            User user = await theVEYMService.getUserAsync(membershipID);
            int counter = 0;

            foreach (String trainingID in user.trainingCamps)
            {
                Training training = await theVEYMService.getTrainingAsync(trainingID);

                foreach(TrainingUserItem trainingUserItem in training.signupList)
                {
                    if (trainingUserItem.membershipID.Equals(membershipID))
                    {
                        if(trainingUserItem.status.Equals(Jp2Portal.Helpers.Constants.TRAINING_STATUS_PENDING_REGISTRATION) || trainingUserItem.status.Equals(Jp2Portal.Helpers.Constants.TRAINING_STATUS_REGISTERED))
                        {
                            //Display!

                            ViewData["TrainingID" + counter] = trainingID;
                            ViewData["TrainingName" + counter] = training.trainingName;
                            ViewData["Status" + counter] = trainingUserItem.status;
                            ViewData["Priority" + counter] = trainingUserItem.placeInQueue;
                            ViewData["TimeSignedUp" + counter] = trainingUserItem.timeSignedUp;
                            ViewData["Paid" + counter] = "No";
                            counter++;
                        }
                    }
                }
            }

            ViewData["MembershipID"] = membershipID;

            ViewData["NumberOfTrainings"] = counter - 1;
            //Loop though Training Camps for all that are Constants.TRAINING_STATUS_PENDING_REGISTRATION or Constants.TRAINING_STATUS_REGISTERED
            //Query for that camp

            return View();
        }

        public async Task<IActionResult> TrainingsOffered()
        {

            string[] fullPathInParts = Request.Path.Value.Split("/");
            //Iterate through ALL of the trainings and Display Them

            List<Training> listOfAllTrainings;
            string inputParam = fullPathInParts[fullPathInParts.Length - 1];

            //Plauged by the weirdController Bug, Sign up for the Training
            if (inputParam.Contains("S"))
            {
                listOfAllTrainings = await theVEYMService.getAllTrainingAsync();

                string cleanInput = inputParam.Replace("S","");
                string[] inputParts = cleanInput.Split("X");
                string trainingID = inputParts[0];
                string membershipID = inputParts[1];

                string response = await theVEYMService.signUpUserToTrainingAsync(trainingID, membershipID);
                Training training = await theVEYMService.getTrainingAsync(trainingID);

                if (response.Contains(":"))
                {
                    //Get the training name + place in the queue!
                    ViewData["Message"] = "You have sucessfully signed up for " + training.trainingName + "! Your Priority is" + response.Substring(response.LastIndexOf(':'));
                }
                else
                {
                    ViewData["Message"] = "Error in signing up for: " + training.trainingName;
                }
            }
            else
            {
                String membershipID = inputParam;
                ViewData["MembershipID"] = membershipID;
            }

            //Query For Update
            listOfAllTrainings = await theVEYMService.getAllTrainingAsync();

            ViewData["NumberOfTrainings"] = listOfAllTrainings.Count;

            int counter = 0;
            foreach (Training training in listOfAllTrainings)
            {
                ViewData["TrainingID" + counter] = training.trainingID;
                ViewData["TrainingName" + counter] = training.trainingName;
                ViewData["TrainingCapacity" + counter] = training.trainingUserCapacity;
                ViewData["TrainingCurrentNumberOfUsers" + counter] = training.currentTrainingUserCount;
                counter++;
            }

            return View();
        }

    }
}