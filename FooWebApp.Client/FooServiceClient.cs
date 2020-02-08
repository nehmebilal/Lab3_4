using FooWebApp.DataContracts;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FooWebApp.Client
{
    public class FooServiceClient : IFooServiceClient
    {
        private readonly HttpClient _httpClient;

        public FooServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Student> GetStudent(string id)
        {
            var responseMessage = await _httpClient.GetAsync($"api/students/{id}");
            EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedStudent = JsonConvert.DeserializeObject<Student>(json);
            return fetchedStudent;
        }

        private static void EnsureSuccessOrThrow(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new FooServiceException("", responseMessage.StatusCode);
            }
        }

        public async Task AddStudent(Student student)
        {
            string json = JsonConvert.SerializeObject(student);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("api/students", new StringContent(json, Encoding.UTF8,
                "application/json"));
            EnsureSuccessOrThrow(responseMessage);
        }
    }
}
