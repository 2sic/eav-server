using System.Linq;

namespace ToSic.Eav.Data
{
    // ReSharper disable once InconsistentNaming
    public static class IHasDecoratorExtensions
    {
        public static TDecorator GetDecorator<TDecorator, T>(this IHasDecorator<T> parent) where TDecorator: class, IDecorator
        {
            var decorator = parent?.Decorators?.FirstOrDefault(d => d is TDecorator);
            return decorator as TDecorator;
        }

        public static TDecorator GetDecorator<TDecorator>(this IHasDecorator<IEntity> parent) where TDecorator: class, IDecorator
            => parent.GetDecorator<TDecorator, IEntity>();

        public static TDecorator GetDecorator<TDecorator>(this IEntity parent) where TDecorator : class, IDecorator
        {
            if (!(parent is IHasDecorator<IEntity> parentWithDecorator)) return null;
            return parentWithDecorator.GetDecorator<TDecorator, IEntity>();
        }

    }
}
