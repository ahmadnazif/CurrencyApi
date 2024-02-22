using CountryData;

namespace CurrencyApi.Services;

public class Country
{
    private readonly IReadOnlyList<ICountryInfo> meta = CountryLoader.CountryInfo;

    public bool IsCountryCodeExist(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return false;

        var count = meta.ToList().Where(d => d.Iso.Equals(countryCode, StringComparison.CurrentCultureIgnoreCase)).Count();
        return count > 0;
    }

    public List<ICountryInfo> ListAllCountry() => meta.ToList();

    public string? GetCountryName(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        var data = meta.Where(d => d.Iso.Equals(countryCode, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
        if (data == null)
            return null;

        return data.Name;
    }

    public ICountryInfo? GetCountry(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        var data = meta.Where(d => d.Iso.Equals(countryCode, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
        if (data == null)
            return null;

        return data;
    }

    public List<IState> ListAllStates(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return [];

        var data = meta.Where(d => d.Iso.Equals(countryCode, StringComparison.CurrentCultureIgnoreCase)).SingleOrDefault();
        if (data == null)
            return [];

        try
        {
            return [.. CountryLoader.LoadLocationData(countryCode).States];
        }
        catch (Exception)
        {
            return [];
        }
    }
}
