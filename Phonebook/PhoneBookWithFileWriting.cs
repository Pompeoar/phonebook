using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Phonebook
{
    public static class PhoneBookWithFileWriting
    {
        /// <summary>
        /// Appends the given name/number alphabetically.
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        /// <remarks>This keeps only the current read/write in memory.</remarks>
        public static async Task AppendAsync(string fileLocation, string name, string number)
        {
            if (!File.Exists(fileLocation))
            {
                await CreateNewFile(fileLocation, name, number).ConfigureAwait(false);
                return;
            }
            string temp = Path.GetRandomFileName();
            using StreamWriter writer = new(temp, append: true);
            using StreamReader reader = new(fileLocation);
            string line;
            bool stillSearching = true;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (stillSearching && name.CompareTo(line) < 0)
                {
                    await writer.WriteLineAsync($"{name},{number}").ConfigureAwait(false);
                    stillSearching = false;
                }
                await writer.WriteLineAsync(line).ConfigureAwait(false);
            }
            reader.Dispose();
            writer.Dispose();
            File.Move(temp, fileLocation, overwrite: true);
        }

        private static async Task CreateNewFile(string fileLocation, string name, string number)
        {
            using StreamWriter writer = new(fileLocation, append: true);
            await writer.WriteLineAsync($"{name},{number}").ConfigureAwait(false);
        }

        public static async Task<IEnumerable<string>> GetListAsync(string fileLocation, int skip, int take)
        {
            var phoneRecords = new List<string>();
            if (!File.Exists(fileLocation))
            {
                return phoneRecords;
            }
            using StreamReader reader = new(fileLocation);
            string line;
            var index = 1;
            while ((line = await reader.ReadLineAsync()) != null && phoneRecords.Count < take)
            {
                if (index > skip)
                {
                    phoneRecords.Add(line);
                }
                index++;
            }
            return phoneRecords;
        }
    }
}
