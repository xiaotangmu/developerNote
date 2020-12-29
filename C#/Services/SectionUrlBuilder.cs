using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public class SectionUrlBuilder
    {
        public static string BuildHttpBaseUrl(IConfiguration configuration, string baseUrl, string sectionValue)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = configuration.GetSection(sectionValue).Value;
            }
            if (!baseUrl.EndsWith('/') && !baseUrl.EndsWith('\\'))
            {
                baseUrl += "/";
            }
            return baseUrl;
        }
    }
}
