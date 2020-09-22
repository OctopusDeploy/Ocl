using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Octopus.Ocl.Converters
{
    public class DefaultCollectionOclConverter : IOclConverter
    {
        public bool CanConvert(Type type)
            => typeof(IEnumerable).IsAssignableFrom(type);

        public IEnumerable<IOclElement> ToElements(OclConversionContext context, string name, object value)
        {
            var items = (IEnumerable)value;
            foreach (var item in items)
                if (item != null)
                    foreach (var element in context.ToElements(name, item))
                        yield return element;
        }

        public OclDocument ToDocument(OclConversionContext context, object obj)
            => new OclDocument(ToElements(context, "", obj));

        public object? FromElement(OclConversionContext context, Type type, IOclElement element, object? currentValue)
        {
            var collectionType = GetElementType(type);

            var collection = currentValue ?? CreateNewCollection(type, collectionType);

            var item = context.FromElement(collectionType, element, null);

            if (collection is IList list)
            {
                list.Add(item);
                return list;
            }

            var addMethod = collection.GetType().GetMethod("Add");
            if (addMethod == null)
                throw new Exception("Only collections that implement an Add method are supported");

            addMethod.Invoke(collection, new[] { item });

            return collection;
        }

        Type GetElementType(Type type)
        {
            static bool IsIEnumerableOfT(Type t)
                => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>);

            if (type.IsArray)
                return type.GetElementType()!;

            if (IsIEnumerableOfT(type))
                return type.GenericTypeArguments[0];

            var implementsIEnumerableOfT = type
                .GetInterfaces()
                .FirstOrDefault(IsIEnumerableOfT);

            if (implementsIEnumerableOfT != null)
                return implementsIEnumerableOfT.GenericTypeArguments[0];

            throw new Exception($"Could not find element type for {type.Name}, Only arrays and types that implement IEnumerable<T> are supported");
        }

        object CreateNewCollection(Type type, Type collectionType)
        {
            if (type.IsArray)
                return Array.CreateInstance(collectionType, 0);

            if (type.IsInterface)
            {
                var listType = typeof(List<>).MakeGenericType(collectionType);
                if (type.IsAssignableFrom(listType))
                    return Activator.CreateInstance(listType) ?? throw new Exception("Activator returned null");

                throw new Exception("The only interfaces supported for collection properties are those implemented by List<T>");
            }

            return Activator.CreateInstance(type) ?? throw new Exception("Activator returned null");
        }
    }
}