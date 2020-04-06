using FooWebApp.DataContracts;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FooWebApp.Store.DocumentDb
{
    public class DocumentDbStudentStore : IStudentStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);

        public DocumentDbStudentStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        public async Task AddStudent(string courseName, Student student)
        {
            try
            {
                var entity = ToEntity(courseName, student);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                throw new StorageErrorException($"Failed to add student {student.Id} to course {courseName}", e, (int)e.StatusCode);
            }
        }

        public async Task DeleteStudent(string courseName, string studentId)
        {
            try
            {
                await _documentClient.DeleteDocumentAsync(CreateDocumentUri(studentId),
                    new RequestOptions { PartitionKey = new PartitionKey(courseName) });
            }
            catch (DocumentClientException e)
            {
                throw new StorageErrorException($"Failed to delete student {studentId} from course {courseName}", e, (int)e.StatusCode);
            }
        }

        public async Task<Student> GetStudent(string courseName, string studentId)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbStudentEntity>(
                    CreateDocumentUri(studentId),
                    new RequestOptions { PartitionKey = new PartitionKey(courseName) });
                return ToStudent(entity);
            }
            catch (DocumentClientException e)
            {
                throw new StorageErrorException($"Failed to retrieve student {studentId} from course {courseName}", e, (int)e.StatusCode);
            }
        }

        public async Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey(courseName)
                };

                IQueryable<DocumentDbStudentEntity> query = _documentClient.CreateDocumentQuery<DocumentDbStudentEntity>(DocumentCollectionUri, feedOptions)
                    .OrderByDescending(entity => entity.GradePercentage); // much easier ordering than Azure table!

                // you can also add a where statement as follows if you needed to get all students with a minimal grade
                // query = query.Where(entity => entity.GradePercentage > 90);

                FeedResponse<DocumentDbStudentEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbStudentEntity>();
                return new GetStudentsResult
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Students = feedResponse.Select(ToStudent).ToList()
                };
            }
            catch (DocumentClientException e)
            {
                throw new StorageErrorException($"Failed to list students from course {courseName}", e, (int)e.StatusCode);
            }
        }

        public async Task UpdateStudent(string courseName, Student student)
        {
            try
            {
                await _documentClient.UpsertDocumentAsync(DocumentCollectionUri, 
                    ToEntity(courseName, student),
                    new RequestOptions { PartitionKey = new PartitionKey(courseName) });
            }
            catch (DocumentClientException e)
            {
                throw new StorageErrorException($"Failed to upsert student {student.Id} in course {courseName}", e, (int)e.StatusCode);
            }
        }

        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }

        private static DocumentDbStudentEntity ToEntity(string courseName, Student student)
        {
            return new DocumentDbStudentEntity
            {
                Id = student.Id,
                PartitionKey = courseName,
                Name = student.Name,
                GradePercentage = student.GradePercentage
            };
        }

        private static Student ToStudent(DocumentDbStudentEntity entity)
        {
            return new Student
            {
                Id = entity.Id,
                Name = entity.Name,
                GradePercentage = entity.GradePercentage
            };
        }

        class DocumentDbStudentEntity
        {
            public string PartitionKey { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }
            public string Name { get; set; }
            public int GradePercentage { get; set; }
        }
    }
}
