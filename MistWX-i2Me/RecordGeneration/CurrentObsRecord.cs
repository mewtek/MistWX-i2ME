using MistWX_i2Me.API;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me.RecordGeneration;

public class CurrentObsRecord : I2Record
{
    public async Task<string> MakeRecord(List<GenericResponse<CurrentObservationsResponse>> results)
    {
        Log.Info("Creating current observations record..");
        string recordScript = "<Data type=\"CurrentObservations\">";

        foreach (var result in results)
        {
            recordScript +=
                $"<CurrentObservations id=\"000000000\" locationKey=\"{result.Location.primTecci}\" isWxScan=\"0\">" +
                $"{result.RawResponse}<clientKey>{result.Location.primTecci}</clientKey></CurrentObservations>";
        }
        
        recordScript += "</Data>";

        return recordScript;
    }
}