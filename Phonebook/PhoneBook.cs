using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Phonebook
{
    public static class PhoneBook
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 100};
        public static async Task AppendAsync(string fileLocation, string name, string number) =>
            await AppendAsync(fileLocation, new PhoneRecord { Name = name, Number = number });
        public static async Task AppendAsync(string fileLocation, PhoneRecord record)
        {
            var phonebook = await GetPhonebookAsync(fileLocation);
            phonebook = AppendAlphabetically(phonebook, new PhoneRecordNode { record = record });
            await UpdatePhoneBookAsync(fileLocation, phonebook);
        }
        private static async Task<PhoneRecordNode> GetPhonebookAsync(string fileLocation) =>
             JsonSerializer.Deserialize<PhoneRecordNode>(await GetOrStartFileData(fileLocation), options);

        private static async Task UpdatePhoneBookAsync(string fileLocation, PhoneRecordNode phonebook)
        {
            using FileStream createStream = File.Create(fileLocation);
            await JsonSerializer.SerializeAsync(createStream, phonebook, options);
        }
        private static PhoneRecordNode AppendAlphabetically(PhoneRecordNode head, PhoneRecordNode node)
        {
            if (head == null || head.record == null)
            {
                head = node;
                return head;
            }
            PhoneRecordNode current = head;

            if (string.CompareOrdinal(node.record.Name, current.record.Name) < 0)
            {
                node.next = current;
                head = node;
                return head;
            }

            while (current.next != null && string.CompareOrdinal(node.record.Name, current.next.record.Name) > -1)
            {
                current = current.next;
            }
            node.next = current.next;
            current.next = node;
            return head;
        }
        private static async Task<string> GetOrStartFileData(string fileLocation) =>
                File.Exists(fileLocation)
                ? await File.ReadAllTextAsync(fileLocation)
                : "{}";

        public static async Task<IEnumerable<PhoneRecord>> GetListAsync(string fileLocation, int skip, int take)
        {
            var json = await File.ReadAllTextAsync(fileLocation);
            var phonebook = JsonSerializer.Deserialize<PhoneRecordNode>(json);
            return GetList(phonebook, skip, take);
        }

        private static IEnumerable<PhoneRecord> GetList(PhoneRecordNode head, int skip, int take)
        {
            var list = new List<PhoneRecord>();
            if (head == null)
                return list;
            var startingNode = Skip(head, skip);
            list.AddRange(Take(startingNode, take));
            return list;
        }

        private static PhoneRecordNode Skip(PhoneRecordNode head, int skip)
        {
            var current = head;
            var depth = 0;
            while(current.next != null && depth > skip)
            {
                current = current.next;
            }
            return current;
        }

        private static List<PhoneRecord> Take(PhoneRecordNode current, int take)
        {
            var list = new List<PhoneRecord>();
            list.Add(current.record);

            while (current.next != null && list.Count < take)
            {
                current = current.next;
                list.Add(current.record);
            }
            return list;
        }
    }
}
