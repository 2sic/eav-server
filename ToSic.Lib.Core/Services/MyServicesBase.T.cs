namespace ToSic.Lib.Services
{
    /// <summary>
    /// Special type of Service Dependencies which typically replace original dependencies.
    /// They must then still have the original Dependencies to get them.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MyServicesBase<T>: MyServicesBase
    {
        public T RootServices { get; }
        public MyServicesBase(T rootServices)
        {
            // Note: don't add these to log queue, as they will be handled by the base class which needs these dependencies
            RootServices = rootServices;
        }

    }
}
