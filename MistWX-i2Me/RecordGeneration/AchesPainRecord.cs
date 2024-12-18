using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class AchesPainRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<AchesPainResponse>> results)
    {
        Log.Info("Creating Aches & Pain record.");
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "AchesAndPains.xml");
        string recordScript = "<Data type=\"AchesAndPains\">";

        foreach (var result in results)
        {
            recordScript += 
                $"<AchesAndPains id=\"000000000\" locationKey=\"{result.Location.coopId}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.coopId}</clientKey></AchesAndPains>";
        }
        
        recordScript += "</Data>";
        
        await File.WriteAllTextAsync(recordPath, ValidateXml(recordScript));

        return recordPath;
    }
}