using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class PollenRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<PollenResponse>> results)
    {
        Log.Info("Creating Pollen Forecast record.");
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "PollenForecast.xml");
        string recordScript = "<Data type=\"PollenForecast\">";

        foreach (var result in results)
        {
            if (string.IsNullOrEmpty(result.Location.pllnId))
            {
                continue;
            }
            
            recordScript +=
                $"<PollenForecast id=\"000000000\" locationKey=\"{result.Location.pllnId}\" isWxscan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.pllnId}</clientKey></PollenForecast>";
        }
        
        recordScript += "</Data>";
        
        await File.WriteAllTextAsync(recordPath, recordScript);

        return recordPath;
    }
}