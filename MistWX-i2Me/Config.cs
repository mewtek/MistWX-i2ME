using System.Xml.Serialization;

namespace MistWX_i2Me;

/// <summary>
/// Class for building the config.xml file used to configure the software
/// </summary>
[XmlRoot("Config")]
public class Config
{
    [XmlElement] public string TwcApiKey { get; set; } = "REPLACE_ME";

    // Used to process what locations to generate
    [XmlElement]
    public string MachineProductConfig { get; set; } =
        "C:\\Program Files (x86)\\TWC\\i2\\managed\\MachineProductConfig.xml";


    public static Config config = new Config();

    
    /// <summary>
    /// Loads values from the configuration file into the software
    /// </summary>
    /// <returns>Config object</returns>
    public static Config Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "config.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(Config));

        // Create a base config if none exists
        if (!File.Exists(path))
        {
            config = new Config();
            serializer.Serialize(File.Create(path), config);

            return config;
        }

        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            var deserializedConfig = serializer.Deserialize(stream);

            if (deserializedConfig != null && deserializedConfig is Config cfg)
            {
                config = cfg;
                return config;
            }

            return new Config();
        }
        
    }
}