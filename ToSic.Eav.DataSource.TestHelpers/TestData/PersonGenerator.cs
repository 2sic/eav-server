using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using static ToSic.Eav.TestData.PersonSpecs;

namespace ToSic.Eav.TestData;

public class PersonGenerator(DataAssembler dataAssembler, ContentTypeAssembler typeAssembler)
{
    private static Person SemiRandom(PersonSpecs specs, int i)
    {
        var firstName = "First Name " + i;
        var lastName = "Last Name " + i;
        var city = specs.TestCities[i % specs.TestCities.Length];
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
            IsMale = i % specs.IsMaleForEveryX == 0,
            Height = specs.MinHeight + i % specs.HeightVar,
            Modified = RandomData.RandomDate(),
        };
    }

    internal List<Person> GetSemiRandomList(int itemsToGenerate, int firstId, PersonSpecs specs)
    {
        //var persons = new List<Person>();
        //for (var i = firstId; i < firstId + itemsToGenerate; i++)
        //    persons.Add(SemiRandom(Specs, i));

        // now as a functional way
        var persons = Enumerable.Range(firstId, itemsToGenerate)
            .Select(i => SemiRandom(specs, i))
            .ToList();
        return persons;
    }

    internal List<IEntity> Person2Entity(List<Person> persons, bool multiLanguage)
        => persons
            .Select(p => ToEntity(p, multiLanguage))
            .ToList();

    private IEntity ToEntity(Person person, bool multiLanguage)
    {
        // var person = this;
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
        return dataAssembler.CreateEntityTac(appId: 0, entityId: person.Id, contentType: typeAssembler.Type.Transient(PersonTypeName), values: dic,
            titleField: FieldFullName, modified: person.Modified);
    }

    // todo: should be non-static some day, when the test-code isn't static anymore
    private static ILanguage Clone(ILanguage orig, bool readOnly)
        => new LanguageAssembler().CreateFrom(orig, readOnly);

    private object MaybeMakeMl(bool convert, string name, string original)
    {
        if (!convert)
            return original;

        var attribute = dataAssembler.Attribute.CreateTypedAttributeTac(name,  ValueTypes.String, new List<IValue>
        {
            dataAssembler.Value.BuildTac(ValueTypes.String, PriPrefix + original, new List<ILanguage> { LangPri}),
            dataAssembler.Value.BuildTac(ValueTypes.String, EnPrefix + original, new List<ILanguage> { LangEn}),
            dataAssembler.Value.BuildTac(ValueTypes.String, DeMult + original, new List<ILanguage>
            {
                LangDeDe, 
                Clone(LangDeCh, true)
            }),
            dataAssembler.Value.BuildTac(ValueTypes.String, FrPrefix + original, new List<ILanguage> { LangFr })
        });
        return attribute;
    }
    private object MaybeMakeMlBio(bool convert, bool isMale)
    {
        if (!convert)
            return isMale
                ? BioMaleNoLangLast
                : BioFemaleNoLangFirst;

        var valueBioEn = dataAssembler.Value.BuildTac(
            ValueTypes.String,
            isMale ? BioMaleEnLast : BioFemaleEnFirst,
            new List<ILanguage>
            {
                LangEn
            }
        );
        var valueBioDe = dataAssembler.Value.BuildTac(
            ValueTypes.String,
            isMale ? BioMaleDeFirst : BioFemaleDeLast,
            new List<ILanguage>
            {
                LangDeDe,
                Clone(LangDeCh, true)
            });
        var attribute = dataAssembler.Attribute.CreateTypedAttributeTac(
            FieldBioForMlSortTest,
            ValueTypes.String, new List<IValue>
            {
                valueBioEn,
                valueBioDe,
                //ValueBuilder.Build(ValueTypes.String, FrPrefix + original, new List<ILanguage> { LangFr.Copy()})
            });
        return attribute;
    }
    private object MaybeMakeMlNonString<T>(bool convert, string name, ValueTypes type, T original) =>
        !convert
            ? original
            : dataAssembler.Attribute.CreateTypedAttributeTac(name, type, new List<IValue>
            {
                dataAssembler.Value.BuildTac(type, original, DataConstants.NoLanguages),
            });
}