﻿using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace TwentyTwenty.Storage.Google.Test
{
    public class StorageFixture : IDisposable
    {
        public const string ContainerPrefix = "storagetest-";
        public readonly StorageService _client;

        public StorageFixture()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."))
                .AddEnvironmentVariables()
                .AddUserSecrets()
                .Build();

            var credential =
                new ServiceAccountCredential(new ServiceAccountCredential.Initializer(Config["GoogleEmail"])
                {
                    Scopes = new[] {StorageService.Scope.DevstorageFullControl}
                }.FromCertificate(new X509Certificate2(Convert.FromBase64String(Config["GoogleP12PrivateKey"]), "notasecret", X509KeyStorageFlags.Exportable)));

            _client = new StorageService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public IConfiguration Config { get; private set; }

        public void Dispose()
        {
            var blobsToDelete = _client.Objects.List(Config["GoogleBucket"]).Execute().Items
                .WhereToListOrEmpty(b => b.Name.StartsWith(ContainerPrefix));

            foreach (var blob in blobsToDelete)
            {
                _client.Objects.Delete(Config["GoogleBucket"], blob.Name).Execute();
            }
        }
    }
}
