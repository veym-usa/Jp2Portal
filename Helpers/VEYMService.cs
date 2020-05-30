using Jp2Portal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VEYMService.Models;
using VEYMServices.Models;

namespace Jp2Portal.Helpers
{
    public class VEYMService
    {
        //TODO: Derive this programatically
        private String bearerToken = "INSERT BEARER TOKEN HERE";
        private HttpClient client;
        private String baseUri;
        private String usersURI;
        private String trainingsURI;
        private String infoURI;

        public VEYMService()
        {
            //TODO: Get this URL based on environment
            baseUri = "https://localhost:44304";
            //prep to send to the VEYMService
            client = new HttpClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            usersURI = baseUri + "/api/users";
            trainingsURI = baseUri + "/api/training";
            infoURI = baseUri + "/api/info";
        }

        public String getBearerToken()
        {
            return bearerToken;
        }

        public async Task<User> getUserAsync(string membershipID)
        {
            User user = new User();
            user.membershipID = membershipID;

            String jsonRequestBody = JsonConvert.SerializeObject(user);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(usersURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            User responseData = await response.Content.ReadAsAsync<User>();

            return responseData;
        }

        public async Task<List<User>> getAllUserAsync()
        {
            try
            {
                User user = new User();

                String jsonRequestBody = JsonConvert.SerializeObject(user);

                var getRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(usersURI),
                    Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(getRequest);

                List<User> responseData = await response.Content.ReadAsAsync<List<User>>();

                return responseData;

            } catch (Exception e)
            {
                return null;
            }

        }

        public async Task<User> addUserAsync(User user)
        {
            String jsonRequestBody = JsonConvert.SerializeObject(user);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(usersURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            User responseData = await response.Content.ReadAsAsync<User>();

            return responseData;
        }

        public async Task<User> deleteUserAsync(string membershipID)
        {
            User user = new User();
            user.membershipID = membershipID;

            String jsonRequestBody = JsonConvert.SerializeObject(user);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(usersURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            User responseData = await response.Content.ReadAsAsync<User>();

            return responseData;
        }

        public async Task<Training> getTrainingAsync(string trainingID)
        {
            Training training = new Training();
            training.trainingID = trainingID;

            String jsonRequestBody = JsonConvert.SerializeObject(training);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            Training responseData = await response.Content.ReadAsAsync<Training>();

            return responseData;
        }

        public async Task<List<Training>> getAllTrainingAsync()
        {
            Training training = new Training();

            String jsonRequestBody = JsonConvert.SerializeObject(training);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            List<Training> responseData = await response.Content.ReadAsAsync<List<Training>>();

            return responseData;
        }

        public async Task<Training> addTrainingAsync(string trainingID)
        {
            Training training = new Training();
            training.trainingID = trainingID;

            String jsonRequestBody = JsonConvert.SerializeObject(training);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            Training responseData = await response.Content.ReadAsAsync<Training>();

            return responseData;
        }

        public async Task<Training> deleteTrainingAsync(string trainingID)
        {
            Training training = new Training();
            training.trainingID = trainingID;

            String jsonRequestBody = JsonConvert.SerializeObject(training);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            Training responseData = await response.Content.ReadAsAsync<Training>();

            return responseData;
        }

        public async Task<string> signUpUserToTrainingAsync(string trainingID, string membershipID)
        {
            Register register = new Register();
            register.trainingID = trainingID;
            register.membershipID = membershipID;

            String jsonRequestBody = JsonConvert.SerializeObject(register);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            string responseData = await response.Content.ReadAsAsync<string>();

            return responseData;
        }

        public async Task<String> deprioritizeUserInTrainingAsync(string trainingID, string membershipID)
        {
            SignUp signUp = new SignUp();
            signUp.trainingID = trainingID;
            signUp.membershipID = membershipID;
            signUp.sendToTheBack = true;

            String jsonRequestBody = JsonConvert.SerializeObject(signUp);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            String responseData = await response.Content.ReadAsAsync<String>();

            return responseData;
        }

        public async Task<String> dropUserInTrainingAsync(string trainingID, string membershipID)
        {
            SignUp signUp = new SignUp();
            signUp.trainingID = trainingID;
            signUp.membershipID = membershipID;
            signUp.sendToTheBack = false;

            String jsonRequestBody = JsonConvert.SerializeObject(signUp);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(trainingsURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            String responseData = await response.Content.ReadAsAsync<String>();

            return responseData;
        }

        public async Task<Info> addInfoAsync()
        {
            Info info = new Info();

            String jsonRequestBody = JsonConvert.SerializeObject(info);

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(infoURI),
                Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(getRequest);

            Info responseData = await response.Content.ReadAsAsync<Info>();

            return responseData;
        }

        public async Task<Info> getInfoAsync()
        {
            Info responseData = null;

            try {
                Info info = new Info();

                String jsonRequestBody = JsonConvert.SerializeObject(info);

                var getRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(infoURI),
                    Content = new System.Net.Http.StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(getRequest);

                responseData = await response.Content.ReadAsAsync<Info>();
            } 
            catch(Exception e)
            {
                responseData = null;
            }

            return responseData;
        }
    }
}
