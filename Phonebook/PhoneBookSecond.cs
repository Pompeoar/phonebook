using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Phonebook
{
    public static class PhoneBookSecond
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
                using StreamWriter newFile = new(fileLocation, append: true);
                await newFile.WriteLineAsync($"{name},{number}");
                return;
            }
            string temp = Path.GetRandomFileName();          
            using StreamWriter writer = new(temp, append: true);
            using StreamReader reader = new(fileLocation);
            string line;
            bool stillSearching = true;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (stillSearching && name.CompareTo(line) < 0)
                {
                    await writer.WriteLineAsync($"{name},{number}");
                    stillSearching = false;
                }
                await writer.WriteLineAsync(line);
            }
            reader.Dispose();
            writer.Dispose();
            RenameFile(fileLocation, temp);
            File.Delete(temp);
        }

        private static void RenameFile(string fileLocation, string temp)
        {
            File.Move(temp, fileLocation, overwrite: true);
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
