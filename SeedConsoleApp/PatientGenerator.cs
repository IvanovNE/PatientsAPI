using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedConsoleApp
{
    public class PatientGenerator
    {
        private static readonly string[] Genders = { "male", "female", "other", "unknown" };
        private static readonly string[] NameUses = { "usual", "official", "temp", "nickname", "anonymous", "old", "maiden" };
        private static readonly string[] LastNames = { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Михайлов", "Новиков", "Фёдоров" };
        private static readonly string[] FirstNames = { "Иван", "Петр", "Александр", "Михаил", "Дмитрий", "Андрей", "Сергей", "Алексей", "Николай", "Владимир" };
        private static readonly string[] MiddleNames = { "Иванович", "Петрович", "Александрович", "Михайлович", "Дмитриевич" };
        private static readonly string[] FemaleFirstNames = { "Анна", "Мария", "Елена", "Ольга", "Наталья", "Екатерина", "Ирина", "Татьяна", "Светлана", "Юлия" };
        private static readonly string[] FemaleMiddleNames = { "Ивановна", "Петровна", "Александровна", "Михайловна", "Дмитриевна" };

        private readonly Random _random = new();

        public CreatePatientDto Generate()
        {
            var gender = Genders[_random.Next(Genders.Length)];
            var isMale = gender == "male";

            var useFemaleNames = !isMale;

            var lastName = LastNames[_random.Next(LastNames.Length)];
            if (useFemaleNames && !lastName.EndsWith("а"))
                lastName += "а";

            var firstName = useFemaleNames
                ? FemaleFirstNames[_random.Next(FemaleFirstNames.Length)]
                : FirstNames[_random.Next(FirstNames.Length)];

            var middleName = useFemaleNames
                ? FemaleMiddleNames[_random.Next(FemaleMiddleNames.Length)]
                : MiddleNames[_random.Next(MiddleNames.Length)];

            var birthDate = DateTime.UtcNow.AddDays(-_random.Next(1, 365));
            var birthDateUtc = DateTime.SpecifyKind(birthDate, DateTimeKind.Utc);

            return new CreatePatientDto
            {
                Name = new CreateHumanNameDto
                {
                    Family = lastName,
                    Given = new List<string> { firstName, middleName },
                    Use = NameUses[_random.Next(NameUses.Length)]
                },
                Gender = gender,
                BirthDate = birthDateUtc.ToString("yyyy-MM-ddTHH:mm:ss"),
                Active = _random.Next(100) < 90
            };
        }
    }
}
