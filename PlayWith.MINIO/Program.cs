﻿using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;

namespace PlayWith.MINIO
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var endpoint = "localhost:9000";
            var accessKey = "minioadmin";
            var secretKey = "minioadmin";
            try
            {
                var minio = new MinioClient()
                                    .WithEndpoint(endpoint)
                                    .WithCredentials(accessKey, secretKey)
                                   // .WithSSL()
                                    .Build();

                // Create an async task for listing buckets.
                //var getListBucketsTask = await minio.ListBucketsAsync().ConfigureAwait(false);

                //// Iterate over the list of buckets.
                //foreach (var bucket in getListBucketsTask.Buckets)
                //{
                //    Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
                //}


                Run(minio).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        // File uploader task.
        private async static Task Run(IMinioClient minio)
        {
            var bucketName = "test3";
            var objectName = $"2024-07-10_09-51-04.mp4";
            var filePath = "C:\\Users\\Aleksey\\AppData\\Local\\Modem.Geo.VideoMonitoring\\38\\2\\2024-07-10_09-51-04.mp4";
            var contentType = "video/mp4";

            string prefix = null;
            bool recursive = true;
            bool versions = false;

            try
            {
                Console.WriteLine("Running example for API: ListObjectsAsync");
                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(recursive)
                    .WithVersions(versions);
                await foreach (var item in minio.ListObjectsEnumAsync(listArgs).ConfigureAwait(false))
                    Console.WriteLine($"Object: {item.Key}");
                Console.WriteLine($"Listed all objects in bucket {bucketName}\n");

                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }

                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);
                var response = await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                Console.WriteLine("Successfully uploaded " + objectName);

                StatObjectArgs statArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);
                var objStat = await minio.StatObjectAsync(statArgs).ConfigureAwait(false);

                Console.WriteLine("Response: " + objStat.ObjectName 
                    + " Etag: " + objStat.ETag 
                    + " Size: " + objStat.Size
                    + " Type: " + objStat.ContentType);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }
    }
}
