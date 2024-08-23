namespace LumiaFoundation.EFRepository.Extensions
{
    public class MariaDbConfiguration(string host, string port, string user, string password, string database, int majorVersion, int minorVersion, int buildVersion)
    {
        private readonly string _server = host ?? throw new ArgumentNullException(nameof(host));
        private readonly string _port = port ?? throw new ArgumentNullException(nameof(port));
        private readonly string _user = user ?? throw new ArgumentNullException(nameof(user));
        private readonly string _password = password ?? throw new ArgumentNullException(nameof(password));
        private readonly string _database = database ?? throw new ArgumentNullException(nameof(database));
        private readonly int _majorVersion = majorVersion;
        private readonly int _minorVersion = minorVersion;
        private readonly int _buildVersion = buildVersion;

        public string GetConectionString()
        {
            var connectionString = $"server={_server};port={_port};user={_user};password={_password};database={_database}";
            Console.WriteLine(connectionString);
            return connectionString;
        }

        public int MajorVersion => _majorVersion;
        public int MinorVersion => _minorVersion;
        public int BuildVersion => _buildVersion;
    }
}