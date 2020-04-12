﻿using RepoDb.Exceptions;
using RepoDb.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace RepoDb
{
    /// <summary>
    /// A class used to map a type of <see cref="DbConnection"/> into an instance of <see cref="IDbHelper"/> object.
    /// </summary>
    public static class DbHelperMapper
    {
        #region Privates

        private static readonly ConcurrentDictionary<int, IDbHelper> m_maps = new ConcurrentDictionary<int, IDbHelper>();
        private static Type m_type = typeof(DbConnection);

        #endregion

        #region Methods

        /*
         * Add
         */

        /// <summary>
        /// Adds a mapping between the type of <see cref="DbConnection"/> and an instance of <see cref="IDbHelper"/> object.
        /// </summary>
        /// <typeparam name="TDbConnection">The type of <see cref="DbConnection"/> object.</typeparam>
        /// <param name="dbHelper">The instance of <see cref="IDbHelper"/> object to mapped to.</param>
        /// <param name="override">Set to true if to override the existing mapping, otherwise an exception will be thrown if the mapping is already present.</param>
        public static void Add<TDbConnection>(IDbHelper dbHelper,
            bool @override)
            where TDbConnection : DbConnection =>
            Add(typeof(TDbConnection), dbHelper, @override);

        /// <summary>
        /// Adds a mapping between the type of <see cref="DbConnection"/> and an instance of <see cref="IDbHelper"/> object.
        /// </summary>
        /// <param name="connectionType">The type of <see cref="DbConnection"/> object.</param>
        /// <param name="dbHelper">The instance of <see cref="IDbHelper"/> object to mapped to.</param>
        /// <param name="override">Set to true if to override the existing mapping, otherwise an exception will be thrown if the mapping is already present.</param>
        public static void Add(Type connectionType,
            IDbHelper dbHelper,
            bool @override)
        {
            // Guard the type
            Guard(connectionType);

            // Variables
            var key = connectionType.FullName.GetHashCode();
            var existing = (IDbHelper)null;

            // Try get the mappings
            if (m_maps.TryGetValue(key, out existing))
            {
                if (@override)
                {
                    // Override the existing one
                    m_maps.TryUpdate(key, dbHelper, existing);
                }
                else
                {
                    // Throw an exception
                    throw new MappingExistsException($"The database helper mapping to provider '{connectionType.FullName}' already exists.");
                }
            }
            else
            {
                // Add to mapping
                m_maps.TryAdd(key, dbHelper);
            }
        }

        /*
         * Get
         */

        /// <summary>
        /// Gets an existing <see cref="IDbHelper"/> object that is mapped to type <see cref="DbConnection"/>.
        /// </summary>
        /// <typeparam name="TDbConnection">The type of <see cref="DbConnection"/>.</typeparam>
        /// <returns>An instance of mapped <see cref="IDbHelper"/></returns>
        public static IDbHelper Get<TDbConnection>()
            where TDbConnection : DbConnection
        {
            return Get(typeof(TDbConnection));
        }

        /// <summary>
        /// Gets an existing <see cref="IDbHelper"/> object that is mapped to type <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="connectionType">The type of <see cref="DbConnection"/> object.</param>
        /// <returns>An instance of mapped <see cref="IDbHelper"/></returns>
        public static IDbHelper Get(Type connectionType)
        {
            // Guard the type
            Guard(connectionType);

            // Variables for the cache
            var value = (IDbHelper)null;

            // get the value
            m_maps.TryGetValue(connectionType.FullName.GetHashCode(), out value);

            // Return the value
            return value;
        }

        /*
         * Remove
         */

        /// <summary>
        /// Removes the mapping between the type of <see cref="DbConnection"/> and an instance of <see cref="IDbHelper"/> object.
        /// </summary>
        /// <typeparam name="TDbConnection">The type of <see cref="DbConnection"/>.</typeparam>
        /// <param name="throwException">If true, it throws an exception if the mapping is not present.</param>
        /// <returns>True if the removal is successful, otherwise false.</returns>
        public static bool Remove<TDbConnection>(bool throwException = true)
            where TDbConnection : DbConnection =>
            Remove(typeof(TDbConnection), throwException);

        /// <summary>
        /// Removes the mapping between the type of <see cref="DbConnection"/> and an instance of <see cref="IDbHelper"/> object.
        /// </summary>
        /// <param name="connectionType">The type of <see cref="DbConnection"/> object.</param>
        /// <param name="throwException">If true, it throws an exception if the mapping is not present.</param>
        /// <returns>True if the removal is successful, otherwise false.</returns>
        public static bool Remove(Type connectionType,
            bool throwException = true)
        {
            // Check the presence
            GuardPresence(connectionType);

            // Variables for cache
            var key = connectionType.FullName.GetHashCode();
            var existing = (IDbHelper)null;
            var result = m_maps.TryRemove(key, out existing);

            // Throws an exception if necessary
            if (result == false && throwException == true)
            {
                throw new MissingMappingException($"There is no mapping defined for '{connectionType.FullName}'.");
            }

            // Return false
            return result;
        }

        /*
         * Clear
         */

        /// <summary>
        /// Clears all the existing cached <see cref="IDbHelper"/> objects.
        /// </summary>
        public static void Clear()
        {
            m_maps.Clear();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Throws an exception if null.
        /// </summary>
        private static void GuardPresence(Type type)
        {
            if (type == null)
            {
                throw new NullReferenceException("Database helper type.");
            }
        }

        /// <summary>
        /// Throws an exception if the type is not a sublcass of type <see cref="DbConnection"/>.
        /// </summary>
        private static void Guard(Type type)
        {
            GuardPresence(type);
            if (type.IsSubclassOf(m_type) == false)
            {
                throw new InvalidTypeException($"Type must be a subclass of '{m_type.FullName}'.");
            }
        }

        #endregion
    }
}
