using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.API.Products;

public class AchesPainProduct : Base
{
    public AchesPainProduct()
    {
        RecordName = "AchesAndPain";
        DataUrl =
            "https://api.weather.com/v2/indices/achePain/daypart/7day?postalCode={zip}&countryCode=US&format=xml&language=en-US&apiKey={apiKey}";
    }

    public async Task<List<GenericResponse<AchesPainResponse>>> Populate(string[] locations)
    {
        return await GetData<AchesPainResponse>(locations);
    }
}