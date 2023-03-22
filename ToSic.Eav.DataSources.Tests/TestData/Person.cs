using System;

namespace ToSic.Eav.DataSourceTests.TestData
{
    internal class Person
    {
        internal Person() { }

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
        // ReSharper disable once UnusedMember.Global
        public string Biography;
        
    }
}
