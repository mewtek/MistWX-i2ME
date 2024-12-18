using System.Text.Json;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.API.Products;

public class AlertDetailsProduct : Base
{
    public AlertDetailsProduct()
    {
        RecordName = "AlertDetails";
        DataUrl = "N/A";    // Handled differently due to the endpoints used
    }

    public async Task<List<GenericResponse<AlertDetailResponse>>> Populate(
        List<GenericResponse<HeadlineResponse>> headlines)
    {
        List<GenericResponse<AlertDetailResponse>> results = new();

        foreach (var headline in headlines)
        {
            foreach (Alert alert in headline.ParsedData.alerts)
            {
                string url =
                    $"https://api.weather.com/v3/alerts/detail?alertId={alert.detailKey}&format=json&language=en-US&apiKey={Config.config.TwcApiKey}";
                string? res = await DownloadRecord(url);

                if (res == null)
                {
                    continue;
                }

                using (var stream = StreamFromString(res))
                {
                    AlertDetailResponse? response = await JsonSerializer.DeserializeAsync<AlertDetailResponse?>(stream);
                    results.Add(new GenericResponse<AlertDetailResponse>(headline.Location, res, response));
                    Log.Debug("NEW ALERT\n" +
                              $"ETN: {alert.eventTrackingNumber}\n" +
                              $"Desc: {alert.eventDescription}\n" +
                              $"Expiration Time Local: {alert.expireTimeLocal.ToString()}\n" +
                              $"Expiration Time UTC: {alert.expireTimeUTC}");
                }
            }
        }

        return results;
    }
}