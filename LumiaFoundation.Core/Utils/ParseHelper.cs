namespace LumiaFoundation.Core.Utils
{
    public static class ParseHelper
    {
        public static int ToIntOrDefault(string value, int defaultValue)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}