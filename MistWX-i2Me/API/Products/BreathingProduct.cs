using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.API.Products;

public class BreathingProduct : Base
{
    public BreathingProduct()
    {
        RecordName = "Breathing";
        DataUrl =
            "https://api.weather.com/v2/indices/breathing/daypart/7day&postalCode={zip}&countryCode=US&format=xml&language=en-US&apiKey={apiKey}";
    }

    public async Task<List<GenericResponse<BreathingResponse>>> Populate(string[] locations)
    {
        return await GetData<BreathingResponse>(locations);
    }
}