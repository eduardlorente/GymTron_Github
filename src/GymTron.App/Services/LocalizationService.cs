using System.Globalization;
using System.Resources;

namespace GymTron.App.Services;

public class LocalizationService
{
    private const string ResourceId = "GymTron.App.Resources.Strings.AppResources";
    private const string PreferencesKey = "app_language";
    
    private static readonly Lazy<ResourceManager> ResourceManagerInstance = new(() => 
        new ResourceManager(ResourceId, typeof(LocalizationService).Assembly));

    public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentCulture;

    public static event EventHandler? CultureChanged;

    public static void Initialize()
    {
        var savedLanguage = Preferences.Get(PreferencesKey, string.Empty);
        
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            SetCulture(new CultureInfo(savedLanguage));
        }
        else
        {
            // Default to Catalan
            SetCulture(new CultureInfo("ca"));
        }
    }

    public static void SetCulture(CultureInfo culture)
    {
        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        
        Preferences.Set(PreferencesKey, culture.Name);
        
        CultureChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string GetString(string key)
    {
        try
        {
            var result = ResourceManagerInstance.Value.GetString(key, CurrentCulture);
            return result ?? key;
        }
        catch
        {
            return key;
        }
    }

    public static List<CultureInfo> GetSupportedCultures()
    {
        return new List<CultureInfo>
        {
            new CultureInfo("ca"),
            new CultureInfo("es"),
            new CultureInfo("en")
        };
    }

    public static string GetLanguageDisplayName(CultureInfo culture)
    {
        return culture.Name switch
        {
            "ca" => GetString("Language_Catalan"),
            "es" => GetString("Language_Spanish"),
            "en" => GetString("Language_English"),
            _ => culture.NativeName
        };
    }
}
