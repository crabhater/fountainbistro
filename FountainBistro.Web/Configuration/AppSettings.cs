namespace FountainBistro.Web.Configuration;

public class AppSettings
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxOrderItems { get; set; } = 50;
    public int MaxCartItems { get; set; } = 100;
}

public class SbpSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
}

public class PushSettings
{
    public string VapidPublicKey { get; set; } = string.Empty;
    public string VapidPrivateKey { get; set; } = string.Empty;
    public string VapidSubject { get; set; } = string.Empty;
}

public class CacheSettings
{
    public int MenuCacheDurationMinutes { get; set; } = 10;
    public int CartCacheDurationMinutes { get; set; } = 30;
}
