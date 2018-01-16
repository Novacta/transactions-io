// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Provides methods to retrieve information or manipulate 
    /// instances of class <see cref="FileManager"/>.
    /// </summary>
    public static class FileManagerReflection
    {
        /// <summary>
        /// Sets the stream of a <see cref="FileManager"/> 
        /// instance.
        /// </summary>
        /// <param name="target">The object whose stream 
        /// will be set.</param>
        /// <param name="value">The value to assign to the stream.</param>
        public static void SetStream<T>(T target, FileStream value)
            where T : FileManager
        {
            var fileManagerType = typeof(FileManager);
            Type type = target.GetType();
            while (type != fileManagerType)
            {
                type = type.BaseType;
            }

            var typeInfo = type.GetTypeInfo();
            var properties = typeInfo.DeclaredProperties;
            var query = from p in properties where p.Name == "ManagedFileStream" select p;
            var property = query.First();
            property.SetValue(target, value);
        }

        /// <summary>
        /// Gets the field having the specified name
        /// for the given object.
        /// </summary>
        /// <param name="obj">The object whose field value will be returned.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>An object containing the value of the field 
        /// for the given object.</returns>
        public static Object GetField<T>(T obj, string fieldName)
            where T : FileManager
        {
            var fileManagerType = typeof(FileManager);
            Type type = obj.GetType();
            while (type != fileManagerType)
            {
                type = type.BaseType;
            }

            var typeInfo = type.GetTypeInfo();
            var fields = typeInfo.DeclaredFields;
            var query = from f in fields where f.Name == fieldName select f;
            var field = query.First();
            return field.GetValue(obj);
        }

        /// <summary>
        /// Invokes the method represented by the 
        /// current instance, using the specified parameters.
        /// </summary>
        /// <param name="obj">
        /// The object on which to invoke the method.
        /// </param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="parameters">
        /// An argument list for the invoked method. 
        /// If there are no parameters, <paramref name="parameters"/> 
        /// should be <b>null</b>.
        ///</param>
        /// <returns>
        /// An object containing the return value of the invoked method.
        ///</returns>
        public static Object Invoke<T>(T obj, string methodName, Object[] parameters)
            where T : FileManager
        {
            var fileManagerType = typeof(FileManager);
            Type type = obj.GetType();
            while (type != fileManagerType)
            {
                type = type.BaseType;
            }

            var typeInfo = type.GetTypeInfo();
            var methods = typeInfo.DeclaredMethods;
            var query = from f in methods where f.Name == methodName select f;
            var method = query.First();
            return method.Invoke(obj, parameters);
        }
    }
}
