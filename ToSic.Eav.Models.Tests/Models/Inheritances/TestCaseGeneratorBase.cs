using ToSic.Eav.Models.TestData;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Inheritances;

public class TestCaseGeneratorBase(MockDataGenerator<MockForInherit> generator)
{
    protected IEnumerable<object[]> CreateTestCases<TAttribute>(TestCaseName[] testCases)
        where TAttribute : Attribute
        => typeof(TestCaseGeneratorBase).Assembly
            .GetTypesWithAttribute<TAttribute>()
            .SelectMany(pair => testCases
                .Select(testCase => new object[]
                {
                    new TestCaseTypeAndName(
                        Name: testCase.Name,
                        Generator: () =>
                            generator.ToModelTac.ToModel(generator.CreateMetadataForDecorator(), pair.Type,
                                testCase.Name),
                        OriginalType: pair.Type,
                        Attribute: pair.Attribute,
                        Notes: testCase.Notes
                    )
                })
            );

}
