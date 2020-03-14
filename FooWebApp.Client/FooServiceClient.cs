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
            await EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedStudent = JsonConvert.DeserializeObject<Student>(json);
            return fetchedStudent;
        }

        public async Task UpdateStudent(Student student)
        {
            var body = new UpdateStudentRequestBody
            {
                Name = student.Name,
                GradePercentage = student.GradePercentage
            };
            
            string json = JsonConvert.SerializeObject(body);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync($"api/students/{student.Id}", new StringContent(json, 
                Encoding.UTF8, "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }

        public async Task<GetStudentsResponse> GetStudents()
        {
            var responseMessage = await _httpClient.GetAsync("api/students");
            await EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetStudentsResponse>(json);
        }

        public async Task DeleteStudent(string id)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/students/{id}");
            await EnsureSuccessOrThrow(responseMessage);
        }

        private async Task EnsureSuccessOrThrow(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                string message = $"{responseMessage.ReasonPhrase}, {await responseMessage.Content.ReadAsStringAsync()}";
                throw new FooServiceException(message, responseMessage.StatusCode);
            }
        }

        public async Task AddStudent(Student student)
        {
            string json = JsonConvert.SerializeObject(student);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("api/students", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }
    }
}
