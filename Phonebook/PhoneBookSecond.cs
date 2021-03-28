﻿using System;
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
            string temp = @"c:\Dev\temp.txt";
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
            if (!File.Exists(fileLocation))
            {
                using StreamWriter file = new(fileLocation, append: true);
                await file.WriteLineAsync($"{name}\t{number}");
                return;
            }
            using StreamReader reader = new(fileLocation);
            using StreamWriter writer = new(temp, append: true);
            string line;
            bool stillSearching = true;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (stillSearching && name.CompareTo(line) < 0)
                {
                    await writer.WriteLineAsync($"{name}\t{number}");
                }
                await writer.WriteLineAsync(line);
            }
            reader.Dispose();
            writer.Dispose();
            RenameFile(fileLocation, temp);

        }

        private static void RenameFile(string fileLocation, string temp)
        {
            File.Move(temp, fileLocation, overwrite: true);
        }
    }
}
