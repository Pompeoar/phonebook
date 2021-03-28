using Bogus;
using FluentAssertions;
using Phonebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number);
            }

            // Assert
            var data = await File.ReadAllLinesAsync(fileLocation);
            data.Length
                .Should()
                .Be(recordsToCreate);
            data
                .Should()
                .BeEquivalentTo(records.Select(record => $"{record.Name}\t{record.Number}"));
        }

        [Fact]
        public async Task PhoneBook_AppendAlphabetically()
        {
            // Arrange            
            var records = Enumerable.Range(0, 10)
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
                        .Select(record => $"{record.Name}\t{record.Number}"));
        }
    }
}
