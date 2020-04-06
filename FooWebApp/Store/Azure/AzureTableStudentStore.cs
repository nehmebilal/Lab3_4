using System.Collections.Generic;
using FooWebApp.DataContracts;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System.Linq;
using Newtonsoft.Json;

namespace FooWebApp.Store
{
    public class AzureTableStudentStore : IStudentStore
    {
        private readonly CloudTable _table;

        public AzureTableStudentStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(options.Value.StudentsTableName);
        }

        public async Task AddStudent(string courseName, Student student)
        {
            StudentTableEntity entity = ToEntity(courseName, student);
            var insertOperation = TableOperation.Insert(entity);
            try
            {
                await _table.ExecuteAsync(insertOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Failed to add student {student.Id} to course {courseName}", e, e.RequestInformation.HttpStatusCode);
            }
        }

        private static StudentTableEntity ToEntity(string courseName, Student student)
        {
            return new StudentTableEntity
            {
                PartitionKey = courseName,
                RowKey = student.Id,
                GradePercentage = student.GradePercentage,
                Name = student.Name,
            };
        }

        public async Task DeleteStudent(string courseName, string id)
        {
            StudentTableEntity entity = await RetrieveStudentEntity(courseName, id);
            var deleteOperation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(deleteOperation);
        }

        public async Task<Student> GetStudent(string courseName, string id)
        {
            StudentTableEntity entity = await RetrieveStudentEntity(courseName, id);
            return ToStudent(entity);
        }

        private static Student ToStudent(StudentTableEntity entity)
        {
            return new Student
            {
                Id = entity.RowKey,
                GradePercentage = entity.GradePercentage,
                Name = entity.Name
            };
        }

        private async Task<StudentTableEntity> RetrieveStudentEntity(string courseName, string id)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<StudentTableEntity>(partitionKey: courseName, rowkey: id);
            TableResult tableResult = await _table.ExecuteAsync(retrieveOperation);
            var entity = (StudentTableEntity)tableResult.Result;
            if (entity == null)
            {
                throw new StorageErrorException($"Student with id {id} was not found in course {courseName}", 404);
            }
            return entity;
        }

        public async Task<GetStudentsResult> GetStudents(string courseName, string continuationToken, int limit)
        {
            var students = new List<Student>();
            TableContinuationToken tableContinuationToken = null;
            if (!string.IsNullOrWhiteSpace(continuationToken))
            {
                tableContinuationToken = JsonConvert.DeserializeObject<TableContinuationToken>(continuationToken);
            }

            try
            {
                var tableQuery = new TableQuery<StudentTableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, courseName))
                    .Take(limit);
                var queryResult = await _table.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);
                List<StudentTableEntity> entities = queryResult.Results;
                students.AddRange(entities.Select(ToStudent));

                // this doesn't really work, the grades should be sorted by row key because we're only sorting a page here, 
                // not the whole table.
                students.Sort((first, second) => second.GradePercentage.CompareTo(first.GradePercentage));

                return new GetStudentsResult
                {
                    ContinuationToken = queryResult.ContinuationToken == null? null : JsonConvert.SerializeObject(queryResult.ContinuationToken),
                    Students = students
                };
            } 
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not list students in course {courseName}",
                    e.RequestInformation.HttpStatusCode);
            }
        }

        public async Task UpdateStudent(string courseName, Student student)
        {
            var entity = ToEntity(courseName, student);
            TableOperation updateOperation = TableOperation.InsertOrReplace(entity);

            try
            {
                await _table.ExecuteAsync(updateOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not update student in storage, Id = {student.Id}, CourseName {courseName}", 
                    e, e.RequestInformation.HttpStatusCode);
            }
        }
    }
}
