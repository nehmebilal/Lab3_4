using System.Collections.Generic;
using FooWebApp.DataContracts;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using System.Linq;

namespace FooWebApp.Store
{
    public class AzureTableStudentStore : IStudentStore
    {
        private const string _className = "EECE503E";
        private readonly CloudTable _table;

        public AzureTableStudentStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(options.Value.StudentsTableName);
        }

        public async Task AddStudent(Student student)
        {
            StudentTableEntity entity = ToEntity(student);
            var insertOperation = TableOperation.Insert(entity);
            try
            {
                await _table.ExecuteAsync(insertOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // conflict
                {
                    throw new StudentAlreadyExistsException($"Student {student.Id} already exists");
                }
                throw new StorageErrorException("Could not write to Azure Table", e);
            }
        }

        private static StudentTableEntity ToEntity(Student student)
        {
            return new StudentTableEntity
            {
                PartitionKey = _className,
                RowKey = student.Id,
                GradePercentage = student.GradePercentage,
                Name = student.Name,
            };
        }

        public async Task DeleteStudent(string id)
        {
            StudentTableEntity entity = await RetrieveStudentEntity(id);
            var deleteOperation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(deleteOperation);
        }

        public async Task<Student> GetStudent(string id)
        {
            try
            {
                StudentTableEntity entity = await RetrieveStudentEntity(id);
                return ToStudent(entity);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not read from Azure Table", e);
            }
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

        private async Task<StudentTableEntity> RetrieveStudentEntity(string id)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<StudentTableEntity>(partitionKey: _className, rowkey: id);
            TableResult tableResult = await _table.ExecuteAsync(retrieveOperation);
            var entity = (StudentTableEntity)tableResult.Result;
            if (entity == null)
            {
                throw new StudentNotFoundException($"Student with id {id} was not found");
            }
            return entity;
        }

        public async Task<List<Student>> GetStudents()
        {
            var students = new List<Student>();
            TableContinuationToken continuationToken = null;
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(new TableQuery<StudentTableEntity>(), continuationToken);
                List<StudentTableEntity> entities = queryResult.Results;
                students.AddRange(entities.Select(ToStudent));
            } while (continuationToken != null);

            students.Sort((first, second) => second.GradePercentage.CompareTo(first.GradePercentage));
            return students;
        }

        public async Task UpdateStudent(Student student)
        {
            var entity = ToEntity(student);
            TableOperation updateOperation = TableOperation.InsertOrReplace(entity);

            try
            {
                await _table.ExecuteAsync(updateOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 412) // precondition failed
                {
                    throw new StorageConflictException("Optimistic concurrency failed", e);
                }
                throw new StorageErrorException($"Could not student in storage, Id = {student.Id}", e);
            }
        }
    }
}
