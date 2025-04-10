using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection;

public static class FastReflections
{
    private static readonly ConcurrentDictionary<MethodInfo, Delegate> MethodCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Delegate> PropertyGetCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, Delegate> PropertySetCache = new();
    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

    static FastReflections()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
        {
            foreach (var type in args.LoadedAssembly.GetTypes())
                if (type.FullName is not null)
                    TypeCache.TryAdd(type.FullName, type);
        };
    }

    public static TResult? Invoke<TInstance, TResult>(MethodInfo method, TInstance instance, params object[] parameters)
    {
        if (method.IsStatic && instance is null)
            throw new ArgumentNullException(nameof(instance), "Intance could not be null when method is static");

        if (!MethodCache.TryGetValue(method, out var del))
        {
            var instanceParam = Expression.Parameter(typeof(TInstance), "instance");
            var paramExprs = method.GetParameters()
                .Select((p, i) => Expression.Convert(Expression.ArrayIndex(Expression.Constant(parameters), Expression.Constant(i)), p.ParameterType))
                .ToArray();

            var callExpr = Expression.Call(instanceParam, method, paramExprs);
            del = Expression.Lambda<Func<TInstance, TResult>>(Expression.Convert(callExpr, typeof(TResult)), instanceParam).Compile();
            MethodCache[method] = del;
        }
        return ((Func<TInstance, TResult>)del)(instance);
    }

    public static TValue? GetPropertyValue<TInstance, TValue>(TInstance instance, Expression<Func<TInstance, TValue>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo property })
            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

        if (!PropertyGetCache.TryGetValue(property, out var getter))
        {
            var instanceParameter = Expression.Parameter(typeof(TInstance), "instance");

            getter = Expression.Lambda<Func<TInstance, TValue>>(
                Expression.Property(instanceParameter, property),
                instanceParameter
            ).Compile();

            PropertyGetCache[property] = getter;
        }

        return ((Func<TInstance, TValue>)getter)(instance);
    }

    public static void SetPropertyValue<TInstance, TValue>(TInstance instance, Expression<Func<TInstance, TValue>> propertyExpression, TValue value)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo property })
            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

        if (!PropertySetCache.TryGetValue(property, out var setter))
        {
            var instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
            var valueParam = Expression.Parameter(typeof(TValue), "value");

            setter = Expression.Lambda<Action<TInstance, TValue>>(
                Expression.Assign(
                    Expression.Property(instanceParameter, property),
                    valueParam
                ),
                instanceParameter,
                valueParam
            ).Compile();

            PropertySetCache[property] = setter;
        }

        ((Action<TInstance, TValue>)setter)(instance, value);
    }

    public static Type? GetTypeByName(string typeName)
    {
        if (!TypeCache.TryGetValue(typeName, out var type))
        {
            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null); }
                    catch { return Enumerable.Empty<Type>(); }
                })
                .FirstOrDefault(t => t is not null && (t.FullName == typeName || t.Name == typeName));

            if (type?.FullName is not null)
                TypeCache.TryAdd(type.FullName, type);
        }
        return type;
    }
}
