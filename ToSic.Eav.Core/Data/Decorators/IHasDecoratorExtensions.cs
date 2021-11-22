using System.Linq;

namespace ToSic.Eav.Data
{
    // ReSharper disable once InconsistentNaming
    public static class IHasDecoratorExtensions
    {
        public static TDecorator GetDecorator<TDecorator, T>(this IHasDecorators<T> parent) where TDecorator: class, IDecorator
        {
            var decorator = parent?.Decorators?.FirstOrDefault(d => d is TDecorator);
            return decorator as TDecorator;
        }

        public static TDecorator GetDecorator<TDecorator>(this IEntity parent) where TDecorator : class, IDecorator
        {
            if (!(parent is IHasDecorators<IEntity> parentWithDecorator)) return null;
            return parentWithDecorator.GetDecorator<TDecorator, IEntity>();
        }

        //public static TDecorator GetDecorator<TDecorator>(this IContentType parent) where TDecorator : class, IDecorator
        //{
        //    if (!(parent is IHasDecorators<IContentType> parentWithDecorator)) return null;
        //    return parentWithDecorator.GetDecorator<TDecorator, IEntity>();
        //}

    }
}
