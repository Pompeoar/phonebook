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

        [Fact]
        public async Task PhoneBook_Append()
        {
            // Arrange            
            var record = fakerPhoneRecord.Generate();

            // Act            
            await PhoneBookSecond.AppendAsync(fileLocation, record.Name, record.Number);

            // Assert
            var data = await File.ReadAllLinesAsync(fileLocation);
            data.Length
                .Should()
                .Be(1);
            data[0]
                .Should()
                .Contain(record.Name)
                .And
                .Contain(record.Number);
            
        }
    }
}
