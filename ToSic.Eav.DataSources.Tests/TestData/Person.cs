using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSourceTests.TestData
{
    internal class Person
    {
        public int Id;
        public string FullName;
        public string First;
        public string Last;
        public string City;
        public string CityOrNull;
        public bool IsMale;
        public int Height;
        public DateTime Birthday;
        public DateTime? BirthdayOrNull;
        public DateTime Modified;

        public static Person SemiRandom(int i)
        {
            var firstName = "First Name " + i;
            var lastName = "Last Name " + i;
            var city = PersonSpecs.TestCities[i % PersonSpecs.TestCities.Length];
            var year = 1900 + i % 110;
            var month = i % 12 + 1;
            var day = i % 28 + 1;
            var birthday = new DateTime(year, month, day);

            return new Person
            {
                Id = i,
                First = firstName,
                Last = lastName,
                FullName = firstName + " " + lastName,
                City = city,
                CityOrNull = i % 2 == 0 ? null : city,
                Birthday = birthday,
                BirthdayOrNull = i % 2 == 0 ? birthday : null as DateTime?,
                IsMale = i % PersonSpecs.IsMaleForEveryX == 0,
                Height = PersonSpecs.MinHeight + i % PersonSpecs.HeightVar,
                Modified = RandomData.RandomDate(),
            };
        }

        internal static List<Person> GetSemiRandomList(
            int itemsToGenerate = PersonSpecs.DefaultItemsToGenerate, 
            int firstId = PersonSpecs.DefaultRootId)
        {
            var persons = new List<Person>();
            for (var i = firstId; i < firstId + itemsToGenerate; i++)
                persons.Add(Person.SemiRandom(i));
            return persons;
        }

        internal static IEntity Person2Entity(Person person)
        {
            var dic = new Dictionary<string, object>
            {
                {PersonSpecs.FieldFirstName, person.First},
                {PersonSpecs.FieldLastName, person.Last},
                {PersonSpecs.FieldFullName, person.FullName},
                {PersonSpecs.FieldCity, person.City},
                {PersonSpecs.FieldCityMaybeNull, person.CityOrNull},
                {PersonSpecs.FieldBirthday, person.Birthday},
                {PersonSpecs.FieldBirthdayNull, person.BirthdayOrNull},
                {PersonSpecs.FieldIsMale, person.IsMale},
                {PersonSpecs.FieldHeight, person.Height},
            };
            return Build.Entity(dic, appId: 0, id: person.Id, typeName: PersonSpecs.PersonTypeName, modified: person.Modified);
        }

        internal static List<IEntity> Person2Entity(List<Person> persons)
            => persons.Select(Person2Entity).ToList();
    }
}
