using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.API.Products;

public class AlertHeadlinesProduct : Base
{
    public AlertHeadlinesProduct()
    {
        RecordName = "AlertHeadlines";
        DataUrl =
            "https://api.weather.com/v3/aggcommon/v3alertsHeadlines?geocodes={geocodes}&language=en-US&format=json&apiKey={apiKey}";
    }

    public async Task<List<GenericResponse<HeadlineResponse>>> Populate(string[] locations)
    {
        return await GetJsonData<HeadlineResponse>(locations);
    }
}