using System.ComponentModel;

namespace GymTron.App.ViewModels.Entities;

public class BodyWeightHistoryItemViewModel(decimal weight, decimal bodyFatPercentage, DateTime createdOn) : INotifyPropertyChanged
{


    private decimal _weight = weight;
    public string Weight
    {
        get => $"{_weight} kg";
        set
        {
            if (decimal.TryParse(value.Replace(" kg", ""), out decimal parsedWeight))
            {
                _weight = parsedWeight;
                OnPropertyChanged(nameof(Weight));
            }
        }
    }

    private decimal _bodyFatPercentage = bodyFatPercentage;
    public string BodyFatPercentage
    {
        get => $"{_bodyFatPercentage} %";
        set
        {
            if (decimal.TryParse(value.Replace(" %", ""), out decimal parsedBodyFatPercentage))
            {
                _bodyFatPercentage = parsedBodyFatPercentage;
                OnPropertyChanged(nameof(BodyFatPercentage));
            }
        }
    }

    private DateTime _createdOn = createdOn;
    public string CreatedOn
    {
        get => _createdOn.ToString("dd/MM/yyyy");
        set
        {
            if (DateTime.TryParse(value, out DateTime parsedDate))
            {
                _createdOn = parsedDate;
                OnPropertyChanged(nameof(CreatedOn));
            }
        }
    }



    private string _weightColor = "Black";
    public string WeightColor
    {
        get => _weightColor;
        set
        {
            if (_weightColor != value)
            {
                _weightColor = value;
                OnPropertyChanged(nameof(WeightColor));
            }
        }
    }

    private string _bodyFatPercentageColor = "Black";
    public string BodyFatPercentageColor
    {
        get => _bodyFatPercentageColor;
        set
        {
            if (_bodyFatPercentageColor != value)
            {
                _bodyFatPercentageColor = value;
                OnPropertyChanged(nameof(BodyFatPercentageColor));
            }
        }
    }


    public void SetColor(BodyWeightHistoryItemViewModel nextItem)
    {
        if (nextItem != null)
        {
            if(_weight > 0)
                WeightColor = _weight > nextItem._weight ? "Red" : "Green";

            if(_bodyFatPercentage > 0)
                BodyFatPercentageColor = _bodyFatPercentage > nextItem._bodyFatPercentage ? "Red" : "Green";
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;


    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}