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
        public async Task PhonebookWithLinkedList_Benchmark()
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = faker.Generate();
                await PhoneBookWithLinkedList.AppendAsync(fileLocation, record.Name, record.Number);
            }
            // marking for both skip and take 
            await PhoneBookWithLinkedList.GetListAsync(fileLocation, recordsToCreate / 2, recordsToCreate);
        }

        [Benchmark]
        public async Task PhonebookWithFileWriting_Benchmark()
        {
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
            for (int i = 0; i < recordsToCreate; i++)
            {
                var record = faker.Generate();
                await PhoneBookWithFileWriting.AppendAsync(fileLocation, record.Name, record.Number);
            }
            // marking for both skip and take 
            await PhoneBookWithFileWriting.GetListAsync(fileLocation, recordsToCreate / 2, recordsToCreate);
        }
    }
}
