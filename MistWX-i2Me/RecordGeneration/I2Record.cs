using System.Reflection.Metadata;
using System.Xml.Linq;

namespace MistWX_i2Me.RecordGeneration;

public class I2Record
{
    /// <summary>
    /// Ensures that the XML document being processed is valid
    /// </summary>
    /// <param name="raw">Raw XML document</param>
    /// <returns>Formatted XML document as a string</returns>
    public string ValidateXml(string raw)
    {
        try
        {
            XDocument doc = XDocument.Parse(raw);
            return doc.ToString().Replace("\r\n", "\n");
        }
        catch (Exception ex)
        {
            Log.Warning("An error occurred while trying to process a record.");
            Log.Error(ex.Message);
            return raw;
        }
    }
}