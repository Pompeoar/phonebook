using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Bogus;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Phonebook
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class PhoneBookBenchmarks
    {
        int seed = 123;
        int recordsToCreate = 50;
        private Faker<PhoneRecord> faker;
        string fileLocation = @"c:\dev\benchmark_phonebook.json";

        public PhoneBookBenchmarks()
        {
            Randomizer.Seed = new Random(seed);
            faker = new Faker<PhoneRecord>()
                .CustomInstantiator(faker => new PhoneRecord
                {
                    Name = faker.Person.FullName,
                    Number = faker.Person.Phone
                });
        }

        [Benchmark(Baseline = true)]
        public async Task Phonebook_AppendAndGetList()
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = faker.Generate();
                await PhoneBook.AppendAsync(fileLocation, record.Name, record.Number);
            }
            // marking for both skip and take 
            await PhoneBook.GetListAsync(fileLocation, recordsToCreate / 2, recordsToCreate);
        }
    }
}
