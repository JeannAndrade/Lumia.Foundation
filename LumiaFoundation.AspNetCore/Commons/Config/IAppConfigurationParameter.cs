namespace LumiaFoundation.AspNetCore.Commons.Config
{
    public interface IAppConfigurationParameter
    {
        public AppConfigurationParameter.JwtParameters JwtParameter { get; }
        public AppConfigurationParameter.MariaDbParameters MariaDbParameter { get; }
    }
}