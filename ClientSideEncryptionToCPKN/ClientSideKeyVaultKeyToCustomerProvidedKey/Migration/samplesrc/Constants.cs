﻿using System.Configuration;

namespace keyVaultClientSideToCustomerProvidedServerSide
{
    public class Constants
    {
        //Parse config file values
        public static string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public static string clientId = ConfigurationManager.AppSettings["clientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string keyVaultKeyUri = ConfigurationManager.AppSettings["keyVaultKeyUri"];
        public static string keyWrapAlgorithm = ConfigurationManager.AppSettings["keyWrapAlgorithm"];
        public static string containerName = ConfigurationManager.AppSettings["containerName"];
        public static string blobName = ConfigurationManager.AppSettings["blobName"];
        public static string serverSideCustomerProvidedKey = ConfigurationManager.AppSettings["serverSideCustomerProvidedKey"];
    }
}