using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatching
{
    public static class ConfigValueProvider
    {
        private static readonly IConfigurationRoot Configuration;

        static ConfigValueProvider()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }


        public static string Get(string name)
        {
            try
            {
                return Configuration[name];
            }

            catch (Exception)
            {
                return null;

            }
        }


        public static List<string> GetArray(string arrName)
        {
            try
            {
                var myArray = Configuration.GetSection(arrName).AsEnumerable();
                return myArray.Select(pair => pair.Value).Where(x => !String.IsNullOrEmpty(x)).ToList();
            }

            catch (Exception)
            {
                return null;
            }

        }

    }
}
