﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Data;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default
// Create tests with multiple fields to sort
// Create tests with DATEs
// Create tests with Modified!

[TestClass]
public class ValueSort_DateTime: TestBaseEavDataSource
{
    private const int TestVolume = 30;
    private const string Birthdate = PersonSpecs.FieldBirthday;
    private const string BirthdateMaybeNull = PersonSpecs.FieldBirthdayNull;
    private const string ModifiedTest = "InternalModified";
    private const string ModifiedReal = "Modified";
    private readonly ValueSort _testDataGeneratedOutsideTimer;
    private readonly ValueFilterMaker _valueFilterMaker;

    public ValueSort_DateTime()
    {
        _valueFilterMaker = new ValueFilterMaker(this);

        _testDataGeneratedOutsideTimer = _valueFilterMaker.GeneratePersonSourceWithDemoData(TestVolume);
    }


    [TestMethod]
    public void ValueSort_SortFieldDesc_Birthday() => SetupFilterAndRun(Birthdate, true);

    [TestMethod] public void ValueSort_DateNull_Asc() => SetupFilterAndRun(BirthdateMaybeNull, true);

    [TestMethod] public void ValueSort_DateNull_Desc() => SetupFilterAndRun(BirthdateMaybeNull, false);

    [TestMethod] public void ValueSort_Sort_Modified() => SetupFilterAndRun(ModifiedReal, true);

    [TestMethod] public void ValueSort_Sort_Modified_Desc() => SetupFilterAndRun(ModifiedReal, false);

    private void SetupFilterAndRun(string field, bool asc)
    {
        var vf = _testDataGeneratedOutsideTimer;
        vf.Attributes = field;
        vf.Directions = asc ? "a" : "d";
        var result = vf.ListForTests().ToList();
        // check that each following city is same or larger...
        ValidateDateFieldIsSorted(result, field, asc);

    }

    private static void ValidateDateFieldIsSorted(List<IEntity> list, string field, bool asc)
    {
        var previous = list.First().TacGet<DateTime?>(field);
        foreach (var entity in list)
        {
            var next = entity.TacGet<DateTime?>(field);

            if (next == null || previous == null)
            {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                var tempToAddDebuggerline = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            }
            double comp;
            if (next == null && previous == null) comp = 0;
            else if (next == null) comp = -1;
            else if (previous == null) comp = 1;
            else comp = (next.Value - previous.Value).TotalMilliseconds;


            if (asc)
                Assert.IsTrue(comp >= 0, "new " + field + " " + next + " should be = or larger than prev " + previous);
            else
                Assert.IsTrue(comp <=0, "new " + field + " " + next + " should be = or smaller than prev " + previous);
            previous = next;
        }
    }

}