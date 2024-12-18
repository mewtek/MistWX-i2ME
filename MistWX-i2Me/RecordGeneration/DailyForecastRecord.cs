using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class DailyForecastRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<DailyForecastResponse>> results)
    {
        Log.Info("Creating Daily Forecast Record");
        string recordScript = "<Data type=\"DailyForecast\">";

        foreach (var result in results)
        {
            recordScript +=
                $"<DailyForecast id=\"000000000\" locationKey=\"{result.Location.coopId}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.coopId}</clientKey></DailyForecast>";
        }

        recordScript += "</Data>";

        return recordScript;
    }
}