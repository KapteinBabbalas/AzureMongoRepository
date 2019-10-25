using System;
using System.Configuration;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace MongoRepository
{
    /// <summary>
    ///     Internal miscellaneous utility functions.
    /// </summary>
    internal static class Util<U>
    {

        /// <summary>
        ///     Creates and returns a MongoDatabase from the specified url.
        /// </summary>
        /// <param name="url">The url to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified url.</returns>
        private static IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Creates and returns a MongoDatabase from the specified CUSTOM Azure connectionstring.
        /// </summary>
        /// <param name="url">The url to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified url.</returns>
        private static IMongoDatabase GetDatabaseFromAzureUrl(string customAzureConnectionString)
        {
            var databaseName = Regex.Matches(customAzureConnectionString, "([^/]+)/?$")[0].ToString();
            var lastSlashIndex = customAzureConnectionString.LastIndexOf("/");
            var connectionStringWithoutDatbaseName= customAzureConnectionString.Substring(0,  lastSlashIndex );

            var settings = MongoClientSettings.FromUrl(
                new MongoUrl(connectionStringWithoutDatbaseName)
            );
            settings.SslSettings =
                new SslSettings {EnabledSslProtocols = SslProtocols.Tls12};
            var mongoClient = new MongoClient(settings);
            return mongoClient.GetDatabase(databaseName);
        }

        /// <summary>
        ///     Creates and returns an IMongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <returns>Returns an IMongoCollection from the specified type and connectionstring.</returns>
        public static IMongoCollection<T> GetCollectionFromConnectionString<T>(string connectionString)
            where T : IEntity<U>
        {
            return GetCollectionFromConnectionString<T>(connectionString, GetCollectionName<T>());
        }

        /// <summary>
        ///     Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        public static IMongoCollection<T> GetCollectionFromConnectionString<T>(string connectionString,
            string collectionName)
            where T : IEntity<U>
        {
            if (connectionString.Contains("azure.com"))
            {
                 return GetDatabaseFromAzureUrl(connectionString)
                .GetCollection<T>(collectionName);
            }
            
            return GetDatabaseFromUrl(new MongoUrl(connectionString))
                .GetCollection<T>(collectionName);
        }

        /// <summary>
        ///     Creates and returns an IMongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <returns>Returns an IMongoCollection from the specified type and url.</returns>
        public static IMongoCollection<T> GetCollectionFromUrl<T>(MongoUrl url)
            where T : IEntity<U>
        {
            return GetCollectionFromUrl<T>(url, GetCollectionName<T>());
        }

        /// <summary>
        ///     Creates and returns an IMongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns an IMongoCollection from the specified type and url.</returns>
        public static IMongoCollection<T> GetCollectionFromUrl<T>(MongoUrl url, string collectionName)
            where T : IEntity<U>
        {
            return GetDatabaseFromUrl(url)
                .GetCollection<T>(collectionName);
        }

        /// <summary>
        ///     Determines the collectionname for T and assures it is not empty
        /// </summary>
        /// <typeparam name="T">The type to determine the collectionname for.</typeparam>
        /// <returns>Returns the collectionname for T.</returns>
        private static string GetCollectionName<T>() where T : IEntity<U>
        {
            string collectionName;
            if (typeof(T).BaseType.Equals(typeof(object)))
                collectionName = GetCollectioNameFromInterface<T>();
            else
                collectionName = GetCollectionNameFromType(typeof(T));

            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentException("Collection name cannot be empty for this entity");
            return collectionName;
        }

        /// <summary>
        ///     Determines the collectionname from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the collectionname from.</typeparam>
        /// <returns>Returns the collectionname from the specified type.</returns>
        private static string GetCollectioNameFromInterface<T>()
        {
            string collectionname;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = Attribute.GetCustomAttribute(typeof(T), typeof(CollectionName));
            if (att != null)
                collectionname = ((CollectionName) att).Name;
            else
                collectionname = typeof(T).Name;

            return collectionname;
        }

        /// <summary>
        ///     Determines the collectionname from the specified type.
        /// </summary>
        /// <param name="entitytype">The type of the entity to get the collectionname from.</param>
        /// <returns>Returns the collectionname from the specified type.</returns>
        private static string GetCollectionNameFromType(Type entitytype)
        {
            string collectionname;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = Attribute.GetCustomAttribute(entitytype, typeof(CollectionName));
            if (att != null)
            {
                // It does! Return the value specified by the CollectionName attribute
                collectionname = ((CollectionName) att).Name;
            }
            else
            {
                if (typeof(Entity).IsAssignableFrom(entitytype))
                    while (!entitytype.BaseType.Equals(typeof(Entity)))
                        entitytype = entitytype.BaseType;
                collectionname = entitytype.Name;
            }

            return collectionname;
        }
    }
}