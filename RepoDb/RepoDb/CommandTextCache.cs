﻿using RepoDb.Enumerations;
using RepoDb.Requests;
using System;
using System.Collections.Concurrent;

namespace RepoDb
{
    /// <summary>
    /// A class used to cache the composed command text used by the library.
    /// </summary>
    internal static class CommandTextCache
    {
        private static readonly ConcurrentDictionary<BaseRequest, string> _cache = new ConcurrentDictionary<BaseRequest, string>();

        /// <summary>
        /// Gets a command text from the cache for <i>Delete</i> operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        public static string GetDeleteText<TEntity>(DeleteRequest request) where TEntity : DataEntity
        {
            var commandText = (string)null;
            if (_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = (request.StatementBuilder ??
                    StatementBuilderMapper.Get(request.Connection?.GetType())?.StatementBuilder ??
                    new SqlDbStatementBuilder());
                commandText = statementBuilder.CreateDelete(queryBuilder: new QueryBuilder<TEntity>(),
                    where: request.Where);
                _cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        /// <summary>
        /// Gets a command text from the cache for <i>Insert</i> operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        public static string GetInsertText<TEntity>(InsertRequest request) where TEntity : DataEntity
        {
            var commandText = (string)null;
            if (_cache.TryGetValue(request, out commandText) == false)
            {
                var primary = PrimaryKeyCache.Get<TEntity>();
                var identity = IdentityCache.Get<TEntity>();
                if (identity != null && identity != primary)
                {
                    throw new InvalidOperationException($"Identity property must be the primary property for type '{typeof(TEntity).FullName}'.");
                }
                var isPrimaryIdentity = (identity != null);
                var statementBuilder = (request.StatementBuilder ??
                    StatementBuilderMapper.Get(request.Connection?.GetType())?.StatementBuilder ??
                    new SqlDbStatementBuilder());
                if (statementBuilder is SqlDbStatementBuilder)
                {
                    var sqlStatementBuilder = ((SqlDbStatementBuilder)statementBuilder);
                    if (isPrimaryIdentity == false)
                    {
                        isPrimaryIdentity = PrimaryKeyIdentityCache.Get<TEntity>(request.Connection.ConnectionString, Command.Insert);
                    }
                    commandText = sqlStatementBuilder.CreateInsert(queryBuilder: new QueryBuilder<TEntity>(),
                        isPrimaryIdentity: isPrimaryIdentity);
                }
                else
                {
                    commandText = statementBuilder.CreateInsert(queryBuilder: new QueryBuilder<TEntity>());
                }
                _cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        /// <summary>
        /// Gets a command text from the cache for <i>Query</i> operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        public static string GetQueryText<TEntity>(QueryRequest request) where TEntity : DataEntity
        {
            var commandText = (string)null;
            if (_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = (request.StatementBuilder ??
                    StatementBuilderMapper.Get(request.Connection?.GetType())?.StatementBuilder ??
                    new SqlDbStatementBuilder());
                commandText = statementBuilder.CreateQuery(queryBuilder: new QueryBuilder<TEntity>(),
                    where: request.Where,
                    orderBy: request.OrderBy,
                    top: request.Top);
                _cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        /// <summary>
        /// Gets a command text from the cache for <i>Update</i> operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        public static string GetUpdateText<TEntity>(UpdateRequest request) where TEntity : DataEntity
        {
            var commandText = (string)null;
            if (_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = (request.StatementBuilder ??
                    StatementBuilderMapper.Get(request.Connection?.GetType())?.StatementBuilder ??
                    new SqlDbStatementBuilder());
                commandText = statementBuilder.CreateUpdate(queryBuilder: new QueryBuilder<TEntity>(),
                    where: request.Where);
                _cache.TryAdd(request, commandText);
            }
            return commandText;
        }
    }
}