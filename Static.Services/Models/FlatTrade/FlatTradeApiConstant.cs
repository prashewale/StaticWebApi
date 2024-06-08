namespace Static.Services.Models.FlatTrade;

public static class FlatTradeApiConstant
{
    public const string PROPERTY_REQUEST_TIME = "request_time";
    public const string PROPERTY_ACCESS_TYPE = "access_type";
    public const string PROPERTY_FULL_NAME = "uname";
    public const string PROPERTY_EMAIL = "email";
    public const string PROPERTY_UID = "uid";
    public const string PROPERTY_MOBILE_NUMBER = "m_num";
    public const string PROPERTY_TOKEN = "susertoken";

    public const string REQUEST_USER_ID = "{user_id}";
    public const string QUICK_AUTH_REQUEST_PASSWORD = "{pwd}";
    public const string QUICK_AUTH_REQUEST_FACTOR2 = "{factor2}";
    public const string QUICK_AUTH_REQUEST_APP_KEY = "{appkey}";
    public const string QUICK_AUTH_REQUEST_SOURCE = "{source}";
    public const string QUICK_AUTH_REQUEST_IMEI = "{imei}";
    public const string QUICK_AUTH_REQUEST_VC = "{vc}";
    public const string QUICK_AUTH_REQUEST_APK_VERSION = "{apkversion}";
    public const string QUICK_AUTH_REQUEST_ADDL_DIV_INF = "{addldivinf}";

    public const string REQUEST_TOKEN = "{token}";

    public const string QUICK_AUTH_REQUEST_BODY_TEMPLATE = $"jData={{\"uid\":\"{REQUEST_USER_ID}\",\"pwd\":\"{QUICK_AUTH_REQUEST_PASSWORD}\",\"factor2\":\"{QUICK_AUTH_REQUEST_FACTOR2}\",\"apkversion\":\"{QUICK_AUTH_REQUEST_APK_VERSION}\",\"imei\":\"{QUICK_AUTH_REQUEST_IMEI}\",\"vc\":\"{QUICK_AUTH_REQUEST_VC}\",\"appkey\":\"{QUICK_AUTH_REQUEST_APP_KEY}\",\"source\":\"{QUICK_AUTH_REQUEST_SOURCE}\",\"addldivinf\":\"{QUICK_AUTH_REQUEST_ADDL_DIV_INF}\"}}";
    public const string USER_DETAILS_REQUEST_BODY_TEMPLATE = $"jData={{\"uid\":\"{REQUEST_USER_ID}\"}}&jKey={REQUEST_TOKEN}";

    // appsettings.json property name
    public const string FLATTRADE_CREDENTIAL_PROPERTY_NAME = "FlatTradeCredential";

    public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
}
