using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.API.Products;

public class AlertHeadlinesProduct : Base
{
    public AlertHeadlinesProduct()
    {
        RecordName = "AlertHeadlines";
        DataUrl =
            "https://api.weather.com/v3/alerts/headlines?areaId={cntyCd}:US&format=json&language=en-US&apiKey={apiKey}";
    }

    public async Task<List<GenericResponse<HeadlineResponse>>> Populate(string[] locations)
    {
        return await GetJsonData<HeadlineResponse>(locations);
    }
}