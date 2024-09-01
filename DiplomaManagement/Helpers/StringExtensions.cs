namespace DiplomaManagement.Helpers
{
    public static class StringExtensions
    {
        public static string Truncate(this string input, int length)
        {
            if (string.IsNullOrEmpty(input)) return "--";
            return input.Length > length ? input.Substring(0, length) + "..." : input;
        }
    }
}
