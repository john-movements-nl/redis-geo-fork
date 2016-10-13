using System.Collections.Generic;
using System.IO;
using System;
using Funq;
using ServiceStack;
using RedisGeo.ServiceInterface;
using ServiceStack.Configuration;
using ServiceStack.Redis;

namespace RedisGeo
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("RedisGeo", typeof(RedisGeoServices).GetAssembly()) {
                 var customSettings = new FileInfo(@"~/appsettings.txt".MapHostAbsolutePath());
                 if(customSettings.Exists) {
                     AppSettings = (IAppSettings)new TextFileSettings(customSettings.FullName);
                 }
            }

        public override void Configure(Container container)
        {
            //Requires GEO commands in v4.0.57 pre-release packages from MyGet: https://github.com/ServiceStack/ServiceStack/wiki/MyGet
            var configuredRedisHost = Environment.GetEnvironmentVariable("AWS_REDIS_HOST") ?? "localhost";
            container.Register<IRedisClientsManager>(c => 
                new RedisManagerPool(AppSettings.Get("RedisHost", defaultValue: $"{configuredRedisHost}:6379")));

            ImportCountry(container.Resolve<IRedisClientsManager>(), "US");
        }

        public void ImportCountry(IRedisClientsManager redisManager, string countryCode)
        {
            using (var redis = redisManager.GetClient())
            using (var reader = new StreamReader(File.OpenRead(MapProjectPath($"~/App_Data/{countryCode}.txt"))))
            {
                string line, lastState = null, lastCity = null;
                var results = new List<ServiceStack.Redis.RedisGeo>();
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    var city = parts[2];
                    var state = parts[4];
                    var latitude = double.Parse(parts[9]);
                    var longitude = double.Parse(parts[10]);

                    if (city == lastCity) //Skip duplicate entries
                        continue;
                    else
                        lastCity = city;

                    if (lastState == null)
                        lastState = state;

                    if (state != lastState)
                    {
                        redis.AddGeoMembers(lastState, results.ToArray());
                        lastState = state;
                        results.Clear();
                    }

                    results.Add(new ServiceStack.Redis.RedisGeo(longitude, latitude, city));
                }
            }
        }
    }
}