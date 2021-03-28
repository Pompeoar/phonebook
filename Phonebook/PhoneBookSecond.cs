using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonebook
{
    public static class PhoneBookSecond
    {
        public static async Task AppendAsync(string fileLocation, string name, string number)
        {
            using StreamWriter file = new(fileLocation, append: true);
            await file.WriteLineAsync(name + "\t" + number);
        }

    }
}
