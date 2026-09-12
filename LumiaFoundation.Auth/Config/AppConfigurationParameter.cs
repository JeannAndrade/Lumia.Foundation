
using Microsoft.Extensions.Configuration;

namespace LumiaFoundation.Auth.Config
{
    public class AppConfigurationParameter(IConfiguration configuration) : IAppConfigurationParameter
    {
        private readonly JwtParameters _jwtParameters = new(configuration);

        public JwtParameters JwtParameter => _jwtParameters;


        public class JwtParameters
        {
            private readonly string _JwtValidIssuer;
            private readonly string _JwtValidAudience;
            private readonly string _JwtSecret;
            private readonly double _JwtExpiresMin;

            public JwtParameters(IConfiguration configuration)
            {
                var jwtSettings = configuration.GetSection("JwtSettings");
                _JwtValidIssuer = jwtSettings["validIssuer"] ?? "LumiaSoftwareAPI";
                _JwtValidAudience = jwtSettings["validAudience"] ?? "https://localhost:5001";
                _JwtExpiresMin = Convert.ToDouble(jwtSettings["expires"]);
                _JwtSecret = configuration["JWTSECRET"] ?? "LumiaSoftwareSecretKey113211162023!!!!";
            }

            public string JwtValidIssuer => _JwtValidIssuer;
            public string JwtValidAudience => _JwtValidAudience;
            public string JwtSecret => _JwtSecret;
            public double JwtExpiresMin => _JwtExpiresMin;
        }
    }
}