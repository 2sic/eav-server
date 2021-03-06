﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using static ToSic.Eav.DataSourceTests.TestData.PersonSpecs;

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
        public string Biography;

        public static Person SemiRandom(int i)
        {
            var firstName = "First Name " + i;
            var lastName = "Last Name " + i;
            var city = TestCities[i % TestCities.Length];
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
                IsMale = i % IsMaleForEveryX == 0,
                Height = MinHeight + i % HeightVar,
                Modified = RandomData.RandomDate(),
            };
        }

        internal static List<Person> GetSemiRandomList(
            int itemsToGenerate = DefaultItemsToGenerate, 
            int firstId = DefaultRootId)
        {
            var persons = new List<Person>();
            for (var i = firstId; i < firstId + itemsToGenerate; i++)
                persons.Add(SemiRandom(i));
            return persons;
        }

        internal static List<IEntity> Person2Entity(List<Person> persons, bool multiLanguage)
            => persons.Select(p => p.ToEntity(multiLanguage)).ToList();

        internal IEntity ToEntity(bool multiLanguage)
        {
            var person = this;
            var dic = new Dictionary<string, object>
            {
                {FieldFirstName, MaybeMakeMl(multiLanguage, FieldFirstName, person.First)},
                {FieldLastName, MaybeMakeMl(multiLanguage, FieldLastName, person.Last)},
                {FieldFullName, MaybeMakeMl(multiLanguage, FieldFullName, person.FullName)},
                {FieldCity, MaybeMakeMl(multiLanguage, FieldCity, person.City)},
                {FieldCityMaybeNull, MaybeMakeMlNonString(multiLanguage, FieldCityMaybeNull,  ValueTypes.String, person.CityOrNull)},
                {FieldBirthday, MaybeMakeMlNonString(multiLanguage, FieldBirthday, ValueTypes.DateTime, person.Birthday)},
                {FieldBirthdayNull, MaybeMakeMlNonString(multiLanguage, FieldBirthdayNull, ValueTypes.DateTime, person.BirthdayOrNull)},
                {FieldIsMale, MaybeMakeMlNonString(multiLanguage, FieldIsMale, ValueTypes.Boolean, person.IsMale)},
                {FieldHeight, MaybeMakeMlNonString(multiLanguage, FieldHeight, ValueTypes.Number, person.Height)},
                {FieldBioForMlSortTest, MaybeMakeMlBio(multiLanguage, person.IsMale)}
            };
            return new Entity(0, person.Id, ContentTypeBuilder.Fake(PersonTypeName), dic,
                FieldFullName, person.Modified);
        }

        private static object MaybeMakeMl(bool convert, string name, string original)
        {
            if (!convert) return original;

            var attribute = AttributeBuilder.CreateTyped(name,  ValueTypes.String, new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, PriPrefix + original, new List<ILanguage> { LangPri.Copy()}),
                ValueBuilder.Build(ValueTypes.String, EnPrefix + original, new List<ILanguage> { LangEn.Copy()}),
                ValueBuilder.Build(ValueTypes.String, DeMult + original, new List<ILanguage>
                    {
                        LangDeDe.Copy(), 
                        LangDeCh.Copy(readOnly: true)
                    }),
                ValueBuilder.Build(ValueTypes.String, FrPrefix + original, new List<ILanguage> { LangFr.Copy()})
            });
            return attribute;
        }
        private static object MaybeMakeMlBio(bool convert, bool isMale)
        {
            if (!convert) return isMale ? BioMaleNoLangLast : BioFemaleNoLangFirst;

            var attribute = AttributeBuilder.CreateTyped(FieldBioForMlSortTest,  ValueTypes.String, new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, isMale ? BioMaleEnLast : BioFemaleEnFirst, new List<ILanguage> { LangEn.Copy()}),
                ValueBuilder.Build(ValueTypes.String, isMale ? BioMaleDeFirst : BioFemaleDeLast, new List<ILanguage>
                    {
                        LangDeDe.Copy(), 
                        LangDeCh.Copy(readOnly: true)
                    }),
                //ValueBuilder.Build(ValueTypes.String, FrPrefix + original, new List<ILanguage> { LangFr.Copy()})
            });
            return attribute;
        }
        private static object MaybeMakeMlNonString<T>(bool convert, string name, ValueTypes type, T original) =>
            !convert
                ? (object) original
                : AttributeBuilder.CreateTyped(name, type, new List<IValue>
                {
                    ValueBuilder.Build(type, original, new List<ILanguage>()),
                });
    }
}
