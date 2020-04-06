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

        public async Task<Student> GetStudent(string courseName, string studentId)
        {
            var responseMessage = await _httpClient.GetAsync($"api/courses/{courseName}/students/{studentId}");
            await EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            var fetchedStudent = JsonConvert.DeserializeObject<Student>(json);
            return fetchedStudent;
        }

        public async Task UpdateStudent(string courseName, Student student)
        {
            var body = new UpdateStudentRequestBody
            {
                Name = student.Name,
                GradePercentage = student.GradePercentage
            };
            
            string json = JsonConvert.SerializeObject(body);
            HttpResponseMessage responseMessage = await _httpClient.PutAsync($"api/courses/{courseName}/students/{student.Id}", new StringContent(json, 
                Encoding.UTF8, "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }

        public Task<GetStudentsResponse> GetStudents(string courseName, int limit)
        {
            return GetStudentsByUri($"api/courses/{courseName}/students?limit={limit}");
        }

        public async Task<GetStudentsResponse> GetStudentsByUri(string uri)
        {
            var responseMessage = await _httpClient.GetAsync(uri);
            await EnsureSuccessOrThrow(responseMessage);
            string json = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetStudentsResponse>(json);
        }

        public async Task DeleteStudent(string courseName, string id)
        {
            var responseMessage = await _httpClient.DeleteAsync($"api/courses/{courseName}/students/{id}");
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

        public async Task AddStudent(string courseName, Student student)
        {
            string json = JsonConvert.SerializeObject(student);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync($"api/courses/{courseName}/students", new StringContent(json, Encoding.UTF8,
                "application/json"));
            await EnsureSuccessOrThrow(responseMessage);
        }
    }
}
