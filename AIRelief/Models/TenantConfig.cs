namespace AIRelief.Models
{
    public class TenantConfig
    {
        public string MarketCode { get; set; } = "relief";
        public string DefaultLanguage { get; set; } = "en";
        public string[] SupportedLanguages { get; set; } = ["en"];
        public string SiteName { get; set; } = "AI Relief";
        public string ThemeFolder { get; set; } = "relief";
        public string HostName { get; set; } = "localhost";
    }
}
