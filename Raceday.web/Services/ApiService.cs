using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Raceday.web.Models;
using System.Net.Http.Headers;

namespace Raceday.web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- AUTHENTICATION ---
        public async Task<(bool Success, UserSessionModel? User, string Message)> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsync($"api/login?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}", null);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic result = JsonConvert.DeserializeObject(content)!;
                var user = new UserSessionModel
                {
                    UserId = result.user.userId != null ? (int)result.user.userId : 0,
                    Fullname = result.user.fullname,
                    Email = result.user.email,
                    Role = result.user.role
                };
                return (true, user, "Login successful.");
            }
            return (false, null, "Invalid email or password.");
        }

        public async Task<(bool Success, UserSessionModel? User, string Message)> RegisterAsync(UserRegistrationViewModel model)
        {
            try
            {
                var jsonPayload = JsonConvert.SerializeObject(model);
                using var httpContent = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/registration/register", httpContent);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var jsonRoot = JsonConvert.DeserializeObject<Dictionary<string, object>>(content, settings);
                    UserSessionModel? user = null;

                    if (jsonRoot != null && jsonRoot.ContainsKey("user"))
                    {
                        var userToken = JsonConvert.SerializeObject(jsonRoot["user"]);
                        user = JsonConvert.DeserializeObject<UserSessionModel>(userToken, settings);
                    }
                    else
                    {
                        user = JsonConvert.DeserializeObject<UserSessionModel>(content, settings);
                    }

                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(user.Role)) user.Role = model.Role;
                        if (string.IsNullOrEmpty(user.Fullname)) user.Fullname = model.Fullname;
                        if (string.IsNullOrEmpty(user.Email)) user.Email = model.Email;
                        return (true, user, "Account successfully created!");
                    }
                    return (false, null, "Registration succeeded, but user session data could not be parsed.");
                }
                return (false, null, string.IsNullOrWhiteSpace(content) ? "Registration failed. Please try again." : $"Server Error: {content}");
            }
            catch (Exception ex)
            {
                return (false, null, $"An error occurred while connecting to the server: {ex.Message}");
            }
        }

        public async Task<List<EventViewModel>> GetEventsByOrganizerAsync(int organizerId)
        {
            var events = await GetAllEventsAsync();
            // Filter events belonging to this specific organizer
            return events.Where(e => e.OrganizerId == organizerId).ToList();
        }

        // --- EVENTS ---
        public async Task<List<EventViewModel>> GetAllEventsAsync()
        {
            var response = await _httpClient.GetAsync("api/event/all");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (content.TrimStart().StartsWith("{"))
                {
                    return new List<EventViewModel>();
                }

                return JsonConvert.DeserializeObject<List<EventViewModel>>(content) ?? new List<EventViewModel>();
            }
            return new List<EventViewModel>();
        }

        public async Task<bool> CreateEventAsync(EventViewModel model, IFormFile? imageFile)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.OrganizerId.ToString()), "organizerId");
            content.Add(new StringContent(model.EventName ?? ""), "eventName");
            content.Add(new StringContent(model.Location ?? ""), "location");
            content.Add(new StringContent(model.EventDate.ToString("yyyy-MM-dd HH:mm:ss")), "eventDate");

            var response = await _httpClient.PostAsync("api/event/create", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateEventAsync(EventViewModel model, IFormFile? imageFile)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.EventId.ToString()), "eventId");
            content.Add(new StringContent(model.OrganizerId.ToString()), "organizerId");
            content.Add(new StringContent(model.EventName), "eventName");
            content.Add(new StringContent(model.Location), "location");
            content.Add(new StringContent(model.EventDate.ToString("o")), "eventDate");

            if (imageFile != null)
            {
                var streamContent = new StreamContent(imageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(streamContent, "imageFile", imageFile.FileName);
            }

            var response = await _httpClient.PutAsync("api/event/update", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteEventAsync(int eventId, int organizerId)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(eventId.ToString()), "eventId");
            content.Add(new StringContent(organizerId.ToString()), "organizerId");

            var request = new HttpRequestMessage(HttpMethod.Delete, "api/event/delete") { Content = content };
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // --- CATEGORIES ---
        public async Task<bool> CreateCategoryAsync(CategoryViewModel model)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.CategoryName), "categoryName");
            content.Add(new StringContent(model.EventId.ToString()), "eventId");
            content.Add(new StringContent(model.Distance), "distance");
            content.Add(new StringContent(model.EventType), "eventType");

            var response = await _httpClient.PostAsync("api/category/create", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryViewModel>> GetCategoriesByEventAsync(int eventId)
        {
            var response = await _httpClient.GetAsync($"api/category/event/{eventId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CategoryViewModel>>(content) ?? new List<CategoryViewModel>();
            }
            return new List<CategoryViewModel>();
        }

        // --- ENROLMENTS ---
        public async Task<bool> CreateEnrolmentAsync(int userId, int categoryId, decimal amount)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(userId.ToString()), "userId");
            content.Add(new StringContent(categoryId.ToString()), "categoryId");
            content.Add(new StringContent(amount.ToString()), "amount");

            var response = await _httpClient.PostAsync("api/enrolment/create", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<EnrolmentViewModel>> GetOrganizerEnrolmentsAsync(int organizerId)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(organizerId.ToString()), "organizerId");

            var response = await _httpClient.PostAsync("api/event/view-enrolments", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                if (json.TrimStart().StartsWith("{"))
                {
                    return new List<EnrolmentViewModel>();
                }

                return JsonConvert.DeserializeObject<List<EnrolmentViewModel>>(json) ?? new List<EnrolmentViewModel>();
            }
            return new List<EnrolmentViewModel>();
        }

        // --- RESULTS ---
        public async Task<bool> CaptureResultAsync(ResultViewModel model)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.EnrolmentId.ToString()), "enrolmentId");
            content.Add(new StringContent(model.FinishTime.ToString()), "finishTime");
            content.Add(new StringContent(model.OverallPosition.ToString()), "overallPosition");
            content.Add(new StringContent(model.CategoryPosition.ToString()), "categoryPosition");

            var response = await _httpClient.PostAsync("api/result/capture", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ResultViewModel>> GetMyResultsAsync(int userId)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(userId.ToString()), "organizerId"); // or userId based on your API model

            var response = await _httpClient.PostAsync("api/result/my-results", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                // If the API returns a message object like {"message": "..."}, treat it as an empty list
                if (json.TrimStart().StartsWith("{"))
                {
                    return new List<ResultViewModel>();
                }

                return JsonConvert.DeserializeObject<List<ResultViewModel>>(json) ?? new List<ResultViewModel>();
            }
            return new List<ResultViewModel>();
        }

        // --- USER PROFILE ---
        public async Task<bool> UpdateProfileAsync(string email, string fullname, string role, string password, IFormFile? profilePicture)
        {
            var url = $"api/update/edit?email={Uri.EscapeDataString(email)}&fullname={Uri.EscapeDataString(fullname)}&role={role}&password={Uri.EscapeDataString(password)}";
            var response = await _httpClient.PutAsync(url, null);
            return response.IsSuccessStatusCode;
        }
    }
}