﻿using RepoDb.Attributes;
using RepoDb.Exceptions;
using RepoDb.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace RepoDb
{
    /// <summary>
    /// A static class that is used to map a class into its equivalent database object (ie: Table, View) column.
    /// This is an alternative class to <see cref="MapAttribute"/> object for property mapping.
    /// </summary>
    public static class PropertyMapper
    {
        #region Privates

        private static readonly ConcurrentDictionary<int, string> m_maps = new ConcurrentDictionary<int, string>();

        #endregion

        #region Methods

        /*
         * Add
         */

        /// <summary>
        /// Adds a mapping between a class property and the database column (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        /// <param name="columnName">The name of the database column.</param>
        public static void Add<TEntity>(Expression<Func<TEntity, object>> expression,
            string columnName)
            where TEntity : class =>
            Add(expression, columnName, false);

        /// <summary>
        /// Adds a mapping between a class property and the database column (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        /// <param name="columnName">The name of the database column.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(Expression<Func<TEntity, object>> expression,
            string columnName,
            bool force)
            where TEntity : class =>
            Add(ExpressionExtension.GetProperty<TEntity>(expression), columnName, force);

        /// <summary>
        /// Adds a mapping between a class property and the database column (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="propertyName">The name of the class property to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        public static void Add<TEntity>(string propertyName,
            string columnName)
            where TEntity : class =>
            Add<TEntity>(propertyName, columnName, false);

        /// <summary>
        /// Adds a mapping between a class property and the database column (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="propertyName">The name of the class property to be mapped.</param>
        /// <param name="columnName">The name of the class property to be mapped.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(string propertyName,
            string columnName,
            bool force)
            where TEntity : class
        {
            // Validates
            ThrowNullReferenceException(propertyName, "PropertyName");

            // Get the property
            var property = TypeExtension.GetProperty<TEntity>(propertyName);
            if (property == null)
            {
                throw new PropertyNotFoundException($"Property '{propertyName}' is not found at type '{typeof(TEntity).FullName}'.");
            }

            // Add to the mapping
            Add(property, columnName, force);
        }

        /// <summary>
        /// Adds a mapping between a class property and the database column (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        public static void Add<TEntity>(Field field,
            string columnName)
            where TEntity : class =>
            Add<TEntity>(field, columnName, false);

        /// <summary>
        /// Adds a mapping between a class property and the database column (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add<TEntity>(Field field,
            string columnName,
            bool force)
            where TEntity : class
        {
            // Validates
            ThrowNullReferenceException(field, "Field");

            // Get the property
            var property = TypeExtension.GetProperty<TEntity>(field.Name);
            if (property == null)
            {
                throw new PropertyNotFoundException($"Property '{field.Name}' is not found at type '{typeof(TEntity).FullName}'.");
            }

            // Add to the mapping
            Add(property, columnName, force);
        }

        /// <summary>
        /// Adds a mapping between a <see cref="ClassProperty"/> object and the database column.
        /// </summary>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        public static void Add(ClassProperty classProperty,
            string columnName) =>
            Add(classProperty.PropertyInfo, columnName, false);

        /// <summary>
        /// Adds a mapping between a <see cref="ClassProperty"/> object and the database column.
        /// </summary>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add(ClassProperty classProperty,
            string columnName,
            bool force) =>
            Add(classProperty?.PropertyInfo, columnName, force);

        /// <summary>
        /// Adds a mapping between a <see cref="PropertyInfo"/> object and the database column.
        /// </summary>
        /// <param name="propertyInfo">The instance of <see cref="PropertyInfo"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        public static void Add(PropertyInfo propertyInfo,
            string columnName) =>
            Add(propertyInfo, columnName, false);

        /// <summary>
        /// Adds a mapping between a <see cref="PropertyInfo"/> object and the database column.
        /// </summary>
        /// <param name="propertyInfo">The instance of <see cref="PropertyInfo"/> to be mapped.</param>
        /// <param name="columnName">The name of the database column.</param>
        /// <param name="force">A value that indicates whether to force the mapping. If one is already exists, then it will be overwritten.</param>
        public static void Add(PropertyInfo propertyInfo,
            string columnName,
            bool force)
        {
            // Validate
            ThrowNullReferenceException(propertyInfo, "PropertyInfo");
            ValidateTargetColumnName(columnName);

            // Variables
            var key = propertyInfo.GenerateCustomizedHashCode();
            var value = (string)null;

            // Try get the cache
            if (m_maps.TryGetValue(key, out value))
            {
                if (force)
                {
                    // Update the existing one
                    m_maps.TryUpdate(key, columnName, value);
                }
                else
                {
                    // Throws an exception
                    throw new MappingExistsException($"A property mapping to '{propertyInfo.DeclaringType.FullName}.{propertyInfo.Name}' already exists.");
                }
            }
            else
            {
                // Add the mapping
                m_maps.TryAdd(key, columnName);
            }
        }

        /*
         * Get
         */

        /// <summary>
        /// Gets the mapped name of the property (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        /// <returns>The mapped name of the property.</returns>
        public static string Get<TEntity>(Expression<Func<TEntity, object>> expression)
            where TEntity : class =>
            Get(ExpressionExtension.GetProperty<TEntity>(expression));

        /// <summary>
        /// Gets the mapped name of the property (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The mapped name of the property.</returns>
        public static string Get<TEntity>(string propertyName)
            where TEntity : class =>
            Get(TypeExtension.GetProperty<TEntity>(propertyName));

        /// <summary>
        /// Gets the mapped name of the property (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> object.</param>
        /// <returns>The mapped name of the property.</returns>
        public static string Get<TEntity>(Field field)
            where TEntity : class =>
            Get(TypeExtension.GetProperty<TEntity>(field.Name));

        /// <summary>
        /// Gets the mapped name of the property via <see cref="ClassProperty"/> object.
        /// </summary>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/>.</param>
        /// <returns>The mapped name of the property.</returns>
        public static string Get(ClassProperty classProperty) =>
            Get(classProperty.PropertyInfo);

        /// <summary>
        /// Gets the mapped name of the property via <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="propertyInfo">The instance of <see cref="PropertyInfo"/>.</param>
        /// <returns>The mapped name of the property.</returns>
        public static string Get(PropertyInfo propertyInfo)
        {
            // Validate
            ThrowNullReferenceException(propertyInfo, "PropertyInfo");

            // Variables
            var key = propertyInfo.GenerateCustomizedHashCode();
            var value = (string)null;

            // Try get the value
            m_maps.TryGetValue(key, out value);

            // Return the value
            return value;
        }

        /*
         * Remove
         */

        /// <summary>
        /// Removes the mapping between the class property and the database column (via expression).
        /// </summary>
        /// <typeparam name="TEntity">The type of the data entity.</typeparam>
        /// <param name="expression">The expression to be parsed.</param>
        public static void Remove<TEntity>(Expression<Func<TEntity, object>> expression)
            where TEntity : class =>
            Remove(ExpressionExtension.GetProperty<TEntity>(expression));

        /// <summary>
        /// Removes the mapping between the class property and database column (via property name).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        public static void Remove<TEntity>(string propertyName)
            where TEntity : class =>
            Remove(TypeExtension.GetProperty<TEntity>(propertyName));

        /// <summary>
        /// Removes the mapping between the  class property and database column (via <see cref="Field"/> object).
        /// </summary>
        /// <typeparam name="TEntity">The target .NET CLR type.</typeparam>
        /// <param name="field">The instance of <see cref="Field"/> object.</param>
        public static void Remove<TEntity>(Field field)
            where TEntity : class =>
            Remove(TypeExtension.GetProperty<TEntity>(field.Name));

        /// <summary>
        /// Removes the mapping between the <see cref="ClassProperty"/> object and the database column.
        /// </summary>
        /// <param name="classProperty">The instance of <see cref="ClassProperty"/>.</param>
        public static void Remove(ClassProperty classProperty) =>
            Remove(classProperty.PropertyInfo);

        /// <summary>
        /// Removes the mapping between the <see cref="PropertyInfo"/> object and the database column.
        /// </summary>
        /// <param name="propertyInfo">The instance of <see cref="PropertyInfo"/>.</param>
        public static void Remove(PropertyInfo propertyInfo)
        {
            // Validate
            ThrowNullReferenceException(propertyInfo, "PropertyInfo");

            // Variables
            var key = propertyInfo.GenerateCustomizedHashCode();
            var value = (string)null;

            // Try get the value
            m_maps.TryRemove(key, out value);
        }

        /*
         * Clear
         */

        /// <summary>
        /// Clears all the existing cached property mapped names.
        /// </summary>
        public static void Clear()
        {
            m_maps.Clear();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Validates the value of the target column name.
        /// </summary>
        /// <param name="columnName">The column name to be validated.</param>
        private static void ValidateTargetColumnName(string columnName)
        {
            if (string.IsNullOrEmpty(columnName?.Trim()))
            {
                throw new NullReferenceException("The target column name cannot be null or empty.");
            }
        }

        /// <summary>
        /// Validates the target object presence.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to be checked.</param>
        /// <param name="argument">The name of the argument.</param>
        private static void ThrowNullReferenceException<T>(T obj,
            string argument)
        {
            if (obj == null)
            {
                throw new NullReferenceException($"The argument '{argument}' cannot be null.");
            }
        }

        #endregion
    }
}
