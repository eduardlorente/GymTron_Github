using System.ComponentModel;
using System.Resources;
using GymTron.App.Services;

namespace GymTron.App.Extensions;

[ContentProperty(nameof(Text))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Text { get; set; } = string.Empty;
    public string? StringFormat { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = "Text",
            Source = new LocalizedText(Text),
            StringFormat = StringFormat
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}

public class LocalizedText : INotifyPropertyChanged
{
    private readonly string _key;

    public LocalizedText(string key)
    {
        _key = key;
        LocalizationService.CultureChanged += (s, e) => OnPropertyChanged(nameof(Text));
    }

    public string Text => LocalizationService.GetString(_key);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
