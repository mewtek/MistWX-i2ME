using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class HeatingCoolingRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<HeatingCoolingResponse>> results)
    {
        Log.Info("Creating Heating & Cooling record.");
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "HeatingAndCooling.xml");
        string recordScript = "<Data type=\"HeatingAndCooling\">";

        foreach (var result in results)
        {
            recordScript +=
                $"<HeatingAndCooling id=\"000000000\" locationKey=\"{result.Location.coopId}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.coopId}</clientKey></HeatingAndCooling>";
        }

        recordScript += "</Data>";

        await File.WriteAllTextAsync(recordPath, ValidateXml(recordScript));

        return recordPath;
    }
}