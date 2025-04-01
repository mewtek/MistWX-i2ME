using System.Data.SQLite;
using Dapper;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Caching.Memory;
using MistWX_i2Me.Schema.System;

namespace MistWX_i2Me.API;

public class Base
{
    protected HttpClient Client = new HttpClient();
    protected string ApiKey = Config.config.TwcApiKey;

    protected string RecordName = String.Empty;
    protected string DataUrl = String.Empty;
    private readonly IMemoryCache locationCache = Globals.LocationCache;

    /// <summary>
    /// Downloads XML data from the specified URL
    /// </summary>
    /// <param name="url">API URL to send a GET request to</param>
    /// <returns>XML Document as a string object</returns>
    public async Task<string> DownloadRecord(string url)
    {
        Log.Debug(url);

        try
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            // response.EnsureSuccessStatusCode();

            Log.Debug(response.StatusCode.ToString());

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Log.Debug("Bad Request issue!");

                return String.Empty;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return String.Empty;
            }


            byte[] content = await Client.GetByteArrayAsync(url);
            string contentString = Encoding.UTF8.GetString(content);

            return contentString;
        }
        catch (Exception ex)
        {
            return String.Empty;
        }

    }

    
    public string GetInnerXml(string content)
    {
        XDocument doc = XDocument.Parse(content);
        XElement? root = doc.Root;
        string innerXml;
        
        if (root == null)
        {
            return String.Empty;
        }

        XmlReader reader = root.CreateReader();
        reader.MoveToContent();
        
        innerXml = reader.ReadInnerXml();
        reader.Close();
        
        return innerXml;
    }

    
    public string FormatUrl(LFRecordLocation location)
    {
        string url = DataUrl;
        url = url.Replace("{apiKey}", ApiKey);

        if (url.Contains("{lat}"))
        {
            url = url.Replace("{lat}", location.lat);
        }

        if (url.Contains("{long}"))
        {
            url = url.Replace("{long}", location.@long);
        }

        if (url.Contains("{zip}"))
        {
            url = url.Replace("{zip}", location.zip2locId);
        }

        if (url.Contains("{zone}"))
        {
            if (location.zoneId == "ZZZ999" || string.IsNullOrEmpty(location.zoneId))
            {
                url = url.Replace("{zone}", location.cntyId);
            }
            else
            {
                url = url.Replace("{zone}", location.zoneId);
            }
        }
        
        return url;
    }


    public async Task<string?> DownloadLocationData(LFRecordLocation location)
    {
        Log.Debug($"Downloading {RecordName} for location {location.locId}");
        
        string url = FormatUrl(location);
        string response = await DownloadRecord(url);
        return response;
    }

    public async Task<LFRecordLocation> GetLocInfo(string locId)
    {
        if (locationCache.TryGetValue(locId, out LFRecordLocation cachedLocation))
        {
            Log.Debug($"Pulled location {locId} from locations cache.");
            return cachedLocation;
        }
        
        SQLiteConnection sqlite =
            new SQLiteConnection($"Data Source={Path.Combine(AppContext.BaseDirectory, "LFRecord.db")}", true);
        
        await sqlite.OpenAsync();

        LFRecordLocation location = await sqlite.QuerySingleAsync<LFRecordLocation>
            ($"SELECT * FROM LFRecord WHERE locId = '{locId}'");
        
        await sqlite.CloseAsync();
        
        locationCache.Set(locId, location);

        Log.Debug($"Location {locId} added to the location cache.");

        return location;
    }

    public static Stream StreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public async Task<List<GenericResponse<T>>> GetData<T>(string[] locations)
    {
        List<GenericResponse<T>> results = new List<GenericResponse<T>>();

        foreach (string location in locations)
        {
            LFRecordLocation locationInfo = await GetLocInfo(location);
            string? response = await DownloadLocationData(locationInfo);

            if (response == null)
            {
                return results;
            }

            string data = GetInnerXml(response);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (StringReader reader = new StringReader(response))
                {
                    var deserializedData = (T?)serializer.Deserialize(reader);

                    if (deserializedData == null)
                    {
                        Log.Warning($"Failed to deserialize {RecordName} for location {location}");
                        continue;
                    }

                    results.Add(new GenericResponse<T>(locationInfo, data, deserializedData));
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.Debug(ex.Message);
                Log.Debug(ex.StackTrace);
                Log.Warning($"Location {location} has no data for {RecordName}, skipping..");
            }
        }

        return results;
    }

    /// <summary>
    /// Creates a list of GenericResponse objects for JSON endpoints.
    /// </summary>
    /// <param name="locations">Array of location IDs</param>
    /// <typeparam name="T">Type of API response</typeparam>
    /// <returns>A list of GenericResponse<T> objects.</returns>
    public async Task<List<GenericResponse<T>>> GetJsonData<T>(string[] locations)
    {
        List<GenericResponse<T>> results = new List<GenericResponse<T>>();

        foreach (string location in locations)
        {
            LFRecordLocation locationInfo = await GetLocInfo(location);
            string? response = await DownloadLocationData(locationInfo);

            if (!string.IsNullOrEmpty(response))
            {
                using (var stream = StreamFromString(response))
                {
                    T? deserializedData = await JsonSerializer.DeserializeAsync<T?>(stream);
                    results.Add(new GenericResponse<T>(locationInfo, response, deserializedData));
                    continue;
                }
            }
            
            Log.Warning($"Failed to generate {RecordName} due to null response.");
        }

        return results;
    }
}