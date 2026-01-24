using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Extensions
{
    public static class ObjectExtensions
    {
        extension(Object from)
        {
            /// <summary>
            /// Copies this object's properties of the same name and type to the given object
            /// </summary>
            /// <typeparam name="TTo">Any object that is a class.</typeparam>
            /// <param name="toObj">The object that will receive the new values.</param>
            /// <returns>The To object that was originally given as a parameter with new values where applicable.</returns>
            public TTo CopyTo<TTo>(TTo toObj)
                where TTo : class
            {
                if (toObj is null) return null;

                var toProps = toObj.GetType().GetProperties().Where(p => p.CanWrite).ToDictionary(p => p.Name, p => p);
                var fromProps = from.GetType().GetProperties().Where(p => p.CanRead);

                foreach (var fromProp in fromProps)
                {
                    if (toProps.TryGetValue(fromProp.Name, out var toProp)
                        && toProp.PropertyType == fromProp.PropertyType
                        && !fromProp.PropertyType.Name.Contains("RepeatedField")                       
                        && !toProp.PropertyType.Name.Contains("RepeatedField")                       
                        )
                    {
                        toProp.SetValue(toObj, fromProp.GetValue(from, null));
                    }
                }

                return toObj;
            }
        }
    }
}
