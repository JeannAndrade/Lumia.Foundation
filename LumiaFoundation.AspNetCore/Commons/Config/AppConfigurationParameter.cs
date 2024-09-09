using Microsoft.Extensions.Configuration;

namespace LumiaFoundation.AspNetCore.Commons.Config
{
    public class AppConfigurationParameter(IConfiguration configuration) : IAppConfigurationParameter
    {
        private readonly JwtParameters _jwtParameters = new(configuration);
        private readonly MariaDbParameters _mariaDbParameters = new(configuration);

        public JwtParameters JwtParameter => _jwtParameters;
        public MariaDbParameters MariaDbParameter => _mariaDbParameters;


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

        public class MariaDbParameters
        {
            private readonly string _dbHost;
            private readonly string _dbPort;
            private readonly string _dbUser;
            private readonly string _dbPassword;
            private readonly string _dbDatabaseName;
            private readonly int _mariaDbMajorVersion;
            private readonly int _mariaDbMinorVersion;
            private readonly int _mariaDbPatchVersion;

            public MariaDbParameters(IConfiguration configuration)
            {
                _dbHost = configuration["DBHOST"] ?? "172.17.0.2";
                _dbPort = configuration["DBPORT"] ?? "3306";
                _dbUser = configuration["DBUSER"] ?? "example-user";
                _dbPassword = configuration["DBPASSWORD"] ?? "my_cool_secret";
                _dbDatabaseName = "financasDb";

                var mariaDbSettings = configuration.GetSection("MariaDbSettings");
                _mariaDbMajorVersion = Convert.ToInt32(mariaDbSettings["major"]);
                _mariaDbMinorVersion = Convert.ToInt32(mariaDbSettings["minor"]);
                _mariaDbPatchVersion = Convert.ToInt32(mariaDbSettings["patch"]);
            }

            public string DbHost => _dbHost;
            public string DbPort => _dbPort;
            public string DbUser => _dbUser;
            public string DbPassword => _dbPassword;
            public string DbDatabaseName => _dbDatabaseName;
            public int MariaDbMajorVersion => _mariaDbMajorVersion;
            public int MariaDbMinorVersion => _mariaDbMinorVersion;
            public int MariaDbPatchVersion => _mariaDbPatchVersion;
        }
    }
}