using Bogus;
using FluentAssertions;
using Phonebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PhonebookTests
{
    public class PhoneBookSecondTests : IDisposable
    {
        Faker<PhoneRecord> fakerPhoneRecord;
        int seed = 123;
        int recordsToCreate = 10;
        string fileLocation = @"c:\dev\unit_test_phonebook.txt";

        public PhoneBookSecondTests()
        {
            Randomizer.Seed = new Random(seed);
            fakerPhoneRecord = new Faker<PhoneRecord>()
                .CustomInstantiator(faker =>
                new PhoneRecord
                {
                    Name = faker.Person.FullName,
                    Number = faker.Person.Phone
                });
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        public void Dispose()
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        [Theory]
        [InlineData(1)] // Can append one record with file missing
        [InlineData(2)] // Can append to existing record
        [InlineData(10)] // Can append repeatedly
        public async Task PhoneBook_Append(int recordsToCreate)
        {
            // Arrange            
            var records = Enumerable.Range(0, recordsToCreate)
                .Select(i => fakerPhoneRecord.Generate())
                .ToList();

            // Act
            foreach (var record in records)
            {
                await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number).ConfigureAwait(false);
            }

            // Assert
            var data = await File.ReadAllLinesAsync(fileLocation);
            data.Length
                .Should()
                .Be(recordsToCreate);
            data
                .Should()
                .BeEquivalentTo(records.Select(record => $"{record.Name},{record.Number}"));
        }

        [Fact]
        public async Task PhoneBook_AppendAlphabetically()
        {
            // Arrange            
            var records = Enumerable.Range(0, recordsToCreate)
                .Select(i => fakerPhoneRecord.Generate())
                 .ToList();

            // Act
            foreach (var record in records)
            {
                await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Assert
            var data = await File.ReadAllLinesAsync(fileLocation);
            data.Length
                .Should()
                .Be(recordsToCreate);
            data
                .Should()
                .Equal(records
                        .OrderBy(record => record.Name)
                        .Select(record => $"{record.Name},{record.Number}"));
        }


        [Theory]
        [InlineData(10, 0, 1, 1)] // Return one record
        [InlineData(10, 0, 10, 10)] // return all records
        [InlineData(10, 0, 9, 9)] // return less than all records
        [InlineData(20, 2, 9, 9)] // skip and return take
        [InlineData(3, 0, 9, 3)] // take exceeds available, returns available
        [InlineData(3, 2, 9, 1)] // skip, take exceeds available, returns available        
        public async Task PhoneBook_GetList(
            int recordsToCreate,
            int skip,
            int take,
            int expectedCount)
        {
            // Arrange            
            var records = Enumerable.Range(0, recordsToCreate)
                .Select(i => fakerPhoneRecord.Generate())
                 .ToList();

            foreach (var record in records)
            {
                await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Act
            IEnumerable<string> phoneRecords = await PhoneBookSecond.GetListAsync(fileLocation, skip, take);

            // Assert
            phoneRecords
                .Should()
                .HaveCount(expectedCount);
            records
                .OrderBy(record => record.Name)
                .Select(record => $"{record.Name},{record.Number}")
                .Skip(skip)
                .Take(take)
                .Should()
                .Equal(phoneRecords);            
        }

        [Fact]
        public async Task PhoneBook_GetLargeList()
        {
            // Arrange            
            var recordsToCreate = 100;
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = fakerPhoneRecord.Generate();
                await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number).ConfigureAwait(false);
            }

            // Act
            IEnumerable<string> phoneRecords = await PhoneBookSecond.GetListAsync(fileLocation, 0, recordsToCreate).ConfigureAwait(false);

            // Assert
            phoneRecords
                .Should()
                .HaveCount(recordsToCreate);
        
        }
    }
}
