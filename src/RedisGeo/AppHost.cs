using System.Collections.Generic;
using System.IO;
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
        /// Configure your ServiceStack AppHost singleton instance:
        /// Call base constructor with App Name and assembly where Service classes are located
        /// </summary>
        public AppHost()
            : base("RedisGeo", typeof(RedisGeoServices).GetAssembly())
        {
            AppSettings = new MultiAppSettings(
                new EnvironmentVariableSettings(),
                new AppSettings());
        }

        public override void Configure(Container container)
        {
            container.Register<IRedisClientsManager>(c =>
                new RedisManagerPool(AppSettings.Get("REDIS_HOST", defaultValue:"localhost")));

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