using System.Xml.Serialization;
using MistWX_i2Me;
using MistWX_i2Me.API;
using MistWX_i2Me.API.Products;
using MistWX_i2Me.Communication;
using MistWX_i2Me.RecordGeneration;
using MistWX_i2Me.Schema.ibm;
using MistWX_i2Me.Schema.System;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("d8b  .d8888b.  888b     d888 8888888888 \nY8P d88P  Y88b 8888b   d8888 888        \n           " +
                          "888 88888b.d88888 888        \n888      .d88P 888Y88888P888 8888888    \n888  .od888P" +
                          "\"  888 Y888P 888 888        " +
                          "\n888 d88P\"      888  Y8P  888 888        \n888 888\"       888   \"   888 888        " +
                          "\n888 888888888  888       " +
                          "888 8888888888 ");
        
        Console.WriteLine("(C) Mist Weather Media");
        Console.WriteLine("This project is licensed under the AGPL v3.0 license.");
        Console.WriteLine("Weather information collected from The National Weather Service & The Weather Company");
        Console.WriteLine("--------------------------------------------------------------------------------------");
        Log.Info("Starting i2ME...");

        Config config = Config.Load();
        
        UdpSender routineSender = new UdpSender(config.UnitConfig.I2MsgAddress, config.UnitConfig.RoutineMsgPort,
                config.UnitConfig.InterfaceAddress);
        UdpSender prioritySender = new UdpSender(config.UnitConfig.I2MsgAddress, config.UnitConfig.PriorityMsgPort,
            config.UnitConfig.InterfaceAddress);

        Log.SetLogLevel(config.LogLevel);
        
        string[] locations = await GetMachineLocations(config);

        Task checkAlerts = TimedTasks.CheckForAlerts(locations, prioritySender);
        Task hourlyRecordGen = TimedTasks.HourlyRecordCollection(locations, routineSender);
        Task cleanTempDir = TimedTasks.CleanTempDirectory();
        await Task.WhenAll(checkAlerts, hourlyRecordGen, cleanTempDir);

    }

    /// <summary>
    /// Runs through the pre-existing MachineProductConfig.xml file to scrape what locations that need
    /// weather information collected.
    /// </summary>
    private static async Task<string[]> GetMachineLocations(Config config)
    {
        List<string> locations = new List<string>();

        Log.Info("Getting locations for this unit..");

        string copyPath = Path.Combine(AppContext.BaseDirectory, "MachineProductConfig.xml");

        if (File.Exists(copyPath))
        {
            File.Delete(copyPath);
        }

        if (!File.Exists(config.MachineProductConfig))
        {
            Log.Error("Unable to locate MachineProductConfig.xml");
            return locations.ToArray();
        }
        
        File.Copy(config.MachineProductConfig, copyPath);

        MachineProductConfig mpc;
        
        using (var reader = new StreamReader(copyPath))
        {
            mpc = (MachineProductConfig) new XmlSerializer(typeof(MachineProductConfig)).Deserialize(reader);
        }

        foreach (ConfigItem i in mpc.ConfigDef.ConfigItems.ConfigItem)
        {
            if (i.Key == "PrimaryLocation" || i.Key == "NearbyLocation1" || i.Key == "NearbyLocation2" ||
                i.Key == "NearbyLocation3" || i.Key == "NearbyLocation4" || i.Key == "NearbyLocation5" ||
                i.Key == "NearbyLocation6" || i.Key == "NearbyLocation7" || i.Key == "NearbyLocation8")
            {
                Log.Debug(i.Value);
                if (string.IsNullOrEmpty(i.Value.ToString()))
                {
                    continue;
                }
                
                string choppedValue = i.Value.ToString().Split("1_US_")[1];
                locations.Add(choppedValue);
                
            }
        }
        
        return locations.ToArray();
    }
}