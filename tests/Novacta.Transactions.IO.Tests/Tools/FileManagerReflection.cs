using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public static void SetStream<T>(T target, FileStream value)
            where T : FileManager
        {
            var createFileManagerType = typeof(FileManager);
            Type type = target.GetType();
            while (type != createFileManagerType)
            {
                type = type.BaseType;
            }

            var typeInfo = type.GetTypeInfo();
            var properties = typeInfo.DeclaredProperties;
            var query = from p in properties where p.Name == "Stream" select p;
            var property = query.First();
            property.SetValue(target, value);
        }

        /// <summary>
        /// Gets the field having the specified name.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the specified field.</returns>
        public static Object GetField<T>(T target, string fieldName)
            where T : FileManager
        {
            var createFileManagerType = typeof(FileManager);
            Type type = target.GetType();
            while (type != createFileManagerType)
            {
                type = type.BaseType;
            }

            var typeInfo = type.GetTypeInfo();
            var fields = typeInfo.DeclaredFields;
            var query = from f in fields where f.Name == fieldName select f;
            var field = query.First();
            return field.GetValue(target);
        }
    }
}
