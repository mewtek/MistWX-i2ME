using MistWX_i2Me.API;
using MistWX_i2Me.API.Products;
using MistWX_i2Me.Communication;
using MistWX_i2Me.RecordGeneration;
using MistWX_i2Me.Schema.ibm;

namespace MistWX_i2Me;

public class TimedTasks
{
    /// <summary>
    /// Cleans out the temp directory every 2 hours.
    /// </summary>
    public static async Task CleanTempDirectory()
    {
        Log.Info("Cleaning temp directory..");

        DirectoryInfo di = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "temp"));
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete();
        }
        
        await Task.Delay(7200 * 1000);
    }

    
    public static async Task HourlyRecordCollection(string[] locations, UdpSender sender)
    {
        while (true)
        {
            Log.Info("Running hourly record collection");
            List<GenericResponse<CurrentObservationsResponse>> obs =
                await new CurrentObservationsProduct().Populate(locations);
            List<GenericResponse<DailyForecastResponse>> dfs = await new DailyForecastProduct().Populate(locations);
            List<GenericResponse<HourlyForecastResponse>> hfs = await new HourlyForecastProduct().Populate(locations);
            List<GenericResponse<AirQualityResponse>> aqs = await new AirQualityProduct().Populate(locations);

            string obsRecord = await new CurrentObsRecord().MakeRecord(obs);
            string dfsRecord = await new DailyForecastRecord().MakeRecord(dfs);
            string hfRecord = await new HourlyForecastRecord().MakeRecord(hfs);
            string aqsRecord = await new AirQualityRecord().MakeRecord(aqs);
            
            
            
            sender.SendFile(obsRecord, "storeData(QGROUP=__CurrentObservations__,Feed=CurrentObservations)");
            sender.SendFile(dfsRecord, "storeData(QGROUP=_DailyForecast__,Feed=DailyForecast)");
            sender.SendFile(hfRecord, "storeData(QGROUP=_HourlyForecast__,Feed=HourlyForecast)");
            sender.SendFile(aqsRecord, "storeData(QGROUP=_AirQuality__,Feed=AirQuality)");
            
            string nextTimestamp = DateTime.Now.AddHours(1).ToString("h:mm tt");
            
            Log.Info($"Next record generation will be at {nextTimestamp}");
            
            await Task.Delay(3600 * 1000);
        }
    }
}