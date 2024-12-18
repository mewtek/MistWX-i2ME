using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class BreathingRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<BreathingResponse>> results)
    {
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "Breathing.xml");
        string recordScript = "<Data type=\"Breathing\">";

        foreach (var result in results)
        {
            recordScript +=
                $"<Breathing id=\"000000000\" locationKey=\"{result.Location.coopId}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.coopId}</clientKey></Breathing>";
        }
        
        recordScript += "</Data>";

        await File.WriteAllTextAsync(recordPath, ValidateXml(recordScript));
        
        return recordPath;
    }
}