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

    /// <summary>
    /// Checks every 10 minutes for new alerts in the unit's area.
    /// </summary>
    /// <param name="locations">Array of location IDs</param>
    /// <param name="sender">UdpSender, prefer priority port</param>
    public static async Task CheckForAlerts(string[] locations, UdpSender sender, int checkInterval)
    {
        if (Config.config.UseNationalLocations)
        {
            Log.Debug("Disabling alert checking since national locations are enabled..");
            return;
        }
        
        while (true)
        {
            Log.Info("Checking for new alerts..");
            List<GenericResponse<HeadlineResponse>> headlines = await new AlertHeadlinesProduct().Populate(locations);

            if (headlines == null || headlines.Count == 0)
            {
                Log.Info("No new alerts found.");
                await Task.Delay(checkInterval * 1000);
                continue;
            }

            List<GenericResponse<AlertDetailResponse>> alerts = await new AlertDetailsProduct().Populate(headlines);

            string? bulletinRecord = await new AlertBulletin().MakeRecord(alerts);
            
            sender.SendFile(bulletinRecord, "storeData(QGROUP=__BERecord__,Feed=BERecord)");
            await Task.Delay(checkInterval * 1000);
        }
    }
    
    public static async Task RecordGenTask(string[] locations, UdpSender sender, int generationInterval)
    {
        while (true)
        {
            Log.Info("Running hourly record collection");
            List<GenericResponse<CurrentObservationsResponse>> obs =
                await new CurrentObservationsProduct().Populate(locations);
            List<GenericResponse<DailyForecastResponse>> dfs = await new DailyForecastProduct().Populate(locations);
            List<GenericResponse<HourlyForecastResponse>> hfs = await new HourlyForecastProduct().Populate(locations);
            List<GenericResponse<AirQualityResponse>> aqs = await new AirQualityProduct().Populate(locations);
            List<GenericResponse<PollenResponse>> pfs = await new PollenForecastProduct().Populate(locations);
            List<GenericResponse<HeatingCoolingResponse>> hcs = await new HeatingCoolingProduct().Populate(locations);
            List<GenericResponse<AchesPainResponse>> acps = await new AchesPainProduct().Populate(locations);
            List<GenericResponse<BreathingResponse>> brs = await new BreathingProduct().Populate(locations);

            string obsRecord = await new CurrentObsRecord().MakeRecord(obs);
            string dfsRecord = await new DailyForecastRecord().MakeRecord(dfs);
            string hfRecord = await new HourlyForecastRecord().MakeRecord(hfs);
            string aqsRecord = await new AirQualityRecord().MakeRecord(aqs);
            string pfsRecord = await new PollenRecord().MakeRecord(pfs);
            string hcRecord = await new HeatingCoolingRecord().MakeRecord(hcs);
            string acpsRecord = await new AchesPainRecord().MakeRecord(acps);
            string brsRecord = await new BreathingRecord().MakeRecord(brs);
            
            sender.SendFile(obsRecord, "storeData(QGROUP=__CurrentObservations__,Feed=CurrentObservations)");
            sender.SendFile(dfsRecord, "storeData(QGROUP=__DailyForecast__,Feed=DailyForecast)");
            sender.SendFile(hfRecord, "storeData(QGROUP=__HourlyForecast__,Feed=HourlyForecast)");
            sender.SendFile(aqsRecord, "storeData(QGROUP=__AirQuality__,Feed=AirQuality)");
            sender.SendFile(pfsRecord, "storeData(QGROUP=__PollenForecast__,Feed=PollenForecast)");
            sender.SendFile(hcRecord, "storeData(QGROUP=__HeatingAndCooling__,Feed=HeatingAndCooling)");
            sender.SendFile(acpsRecord, "storeData(QGROUP=__AchesAndPains__,Feed=AchesAndPains)");
            sender.SendFile(brsRecord, "storeData(QGROUP=__Breathing__,Feed=Breathing)");
            
            string nextTimestamp = DateTime.Now.AddSeconds(generationInterval).ToString("h:mm tt");
            
            Log.Info($"Next record generation will be at {nextTimestamp}");
            
            await Task.Delay(generationInterval * 1000);
        }
    }
}