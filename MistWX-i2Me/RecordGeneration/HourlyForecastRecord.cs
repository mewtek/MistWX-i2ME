using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class HourlyForecastRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<HourlyForecastResponse>> results)
    {
        Log.Info("Creating Hourly Forecast record..");
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "HourlyForecast.xml");
        string recordScript = "<Data type=\"HourlyForecast\">";

        foreach (var result in results)
        {
            recordScript +=
                $"<HourlyForecast id=\"000000000\" locationKey=\"{result.Location.primTecci}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.primTecci}</clientKey></HourlyForecast>";
        }

        recordScript += "</Data>";

        await File.WriteAllTextAsync(recordPath, ValidateXml(recordScript));

        return recordPath;
    }
}