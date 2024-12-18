using System.Drawing;
using Pastel;

namespace MistWX_i2Me;


internal class Log
{
    private static string PREFIX_DEBUG = " [DEBUG] ";
    private static string PREFIX_ERROR = " [ERROR] ";
    private static string PREFIX_WARNING = " [WARN] ";
    private static string PREFIX_INFO = " [INFO] ";
    
    // Colors for console logging
    private static Color COLOR_DEBUG = Color.LightSeaGreen;
    private static Color COLOR_ERROR = Color.Red;
    private static Color COLOR_WARNING = Color.Yellow;
    private static Color COLOR_INFO = Color.Azure;

    private static LogLevel _logLevel = LogLevel.Debug;

    private static string LogStartDate = DateTime.Today.ToString("ddMMyyyy");

    private static string GetCurrentTime()
    {
        return DateTime.Now.ToString(@"MM/dd/yyyy HH:mm:ss");
    }

    public static void SetLogLevel(string newLevel)
    {
        newLevel = newLevel.ToLower();

        switch (newLevel)
        {
            case "debug": 
                Warning("LogLevel is set to debug. This can cause minor slowdowns when generating data.");
                _logLevel = LogLevel.Debug;
                break;
            case "info": _logLevel = LogLevel.Info; break;
            case "warning": _logLevel = LogLevel.Warning; break;
        }
    }


    public static void Debug(string str)
    {
        if (_logLevel > LogLevel.Debug) return;
        str = GetCurrentTime() + PREFIX_DEBUG + str;
        Console.WriteLine(str.Pastel(COLOR_DEBUG));
        WriteLogToFile(str);
    }
    
    public static void Error(string str)
    {
        if (_logLevel > LogLevel.Warning) return;
        str = GetCurrentTime() + PREFIX_ERROR + str;
        Console.WriteLine(str.Pastel(COLOR_ERROR));
        WriteLogToFile(str);
    }
    
    public static void Warning(string str)
    {
        if (_logLevel > LogLevel.Warning) return;
        str = GetCurrentTime() + PREFIX_WARNING + str;
        Console.WriteLine(str.Pastel(COLOR_WARNING));
        WriteLogToFile(str);
    }
    
    public static void Info(string str)
    {
        if (_logLevel > LogLevel.Info) return;
        str = GetCurrentTime() + PREFIX_INFO + str;
        Console.WriteLine(str.Pastel(COLOR_INFO));
        WriteLogToFile(str);
    }


    private static void WriteLogToFile(string str)
    {
        // Create log folder
        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Logs")))
        {
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"));
        }
        
        string fileName = Path.Combine(AppContext.BaseDirectory, "Logs", $"I2ME-{LogStartDate}.log");

        using (StreamWriter sw = File.AppendText(fileName))
        {
            sw.WriteLine(str);
        }
    }

    enum LogLevel
    {
        Debug,
        Info,
        Warning,
    }
}

