using Bogus;
using FluentAssertions;
using Phonebook;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PhonebookTests
{
    public class PhoneBookWithLinkedListTests : IDisposable
    {
        Faker<PhoneRecord> fakerPhoneRecord;
        int seed = 123;
        int recordsToCreate = 10;
        string fileLocation = @"c:\dev\unit_test_phonebook.json";

        public PhoneBookWithLinkedListTests()
        {
            Randomizer.Seed = new Random(seed);
            fakerPhoneRecord = new Faker<PhoneRecord>()
                .CustomInstantiator(faker =>
                new PhoneRecord
                {
                    Name = faker.Person.FullName,
                    Number = faker.Person.Phone
                });
        }

        [Fact]
        public async Task PhoneBook_Append()
        {
            // Arrange
            var record = fakerPhoneRecord.Generate();

            // Act            
            await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);

            // Assert
            var json = File.ReadAllText(fileLocation);
            var phonebook = JsonSerializer.Deserialize<PhoneRecordNode>(json);
            phonebook.record.Name
                .Should()
                .Equals(record.Name);
        }

        [Fact]
        public async Task PhoneBookFile_AppendAndSort()
        {
            // Arrange
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = fakerPhoneRecord.Generate();
                await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Act
            var actualList = await PhoneBookWithLinkedList.GetListAsync(fileLocation, 0, recordsToCreate);

            // Assert
            actualList
                .Should()
                .Equal(actualList.ToList().OrderBy(record => record.Name));

        }


        [Fact]
        public async Task PhoneBook_GetList()
        {
            // Arrange
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = fakerPhoneRecord.Generate();
                await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Act
            var list = await PhoneBookWithLinkedList.GetListAsync(fileLocation, 0, recordsToCreate);

            // Assert
            list.Should()
                .NotBeNull()
                .And
                .HaveCount(recordsToCreate);
        }

        [Fact]
        public async Task PhoneBook_SerializerCanHandleDeepNest()
        {
            // Arrange
            var recordsToCreate = 64; // by default, it will detect a cyclicle reference and throw 
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = fakerPhoneRecord.Generate();
                await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Act
            var list = await PhoneBookWithLinkedList.GetListAsync(fileLocation, 0, recordsToCreate);

            // Assert
            list.Should()
                .NotBeNull()
                .And
                .HaveCount(recordsToCreate);
        }

        [Theory]
        [InlineData(10, 0, 1, 1)] // Return one record
        [InlineData(10, 0, 10, 10)] // return all records
        [InlineData(10, 0, 9, 9)] // return less than all records
        [InlineData(20, 2, 9, 9)] // skip and return take
        [InlineData(3, 2, 9, 1)] // skip and return with less than take available
        [InlineData(1, 2, 1, 1)] // returns take when skip exceeds count
        public async Task PhoneBookFile_SkipAndTake(
            int recordsToCreate,
            int skip,
            int take,
            int expectedCount)
        {
            // Arrange
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = fakerPhoneRecord.Generate();
                await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Act
            var list = await PhoneBookWithLinkedList.GetListAsync(fileLocation, skip, take);

            // Assert
            list.ToList().Distinct()
                .Should()
                .HaveCount(expectedCount);

        }

        public void Dispose()
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }
    }
}
