﻿namespace ToSic.Eav.Data;

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
        return parent is not IHasDecorators<IEntity> parentWithDecorator ? null : parentWithDecorator.GetDecorator<TDecorator, IEntity>();
    }

    public static TDecorator GetDecorator<TDecorator>(this IContentType parent) where TDecorator : class, IDecorator
    {
        return parent is not IHasDecorators<IContentType> parentWithDecorator ? null : parentWithDecorator.GetDecorator<TDecorator, IContentType>();
    }

}