using System.Linq;

namespace ToSic.Eav.Data
{
    // ReSharper disable once InconsistentNaming
    public static class IHasDecoratorExtensions
    {
        public static TDecorator GetDecorator<TDecorator, T>(this IHasDecorator<T> parent) where TDecorator: class
        {
            var decorator = parent?.Decorators?.FirstOrDefault(d => d is TDecorator);
            return decorator as TDecorator;
        }
    }
}
