using System.Xml;
using System.Xml.Serialization;
using Dapper;
using MistWX_i2Me.API;
using MistWX_i2Me.API.Products;
using MistWX_i2Me.Schema.ibm;
using MistWX_i2Me.Schema.System;

namespace MistWX_i2Me.RecordGeneration;

public class AlertBulletin : I2Record
{
    /// <summary>
    /// Dictionary converting alert type keys into IntelliStar 2 vocal keys.
    /// </summary>
    private static Dictionary<string, string> _vocalCodes = new Dictionary<string, string>()
    {
        { "HU_W", "HE001" },
        { "TY_W", "HE002" },
        { "HI_W", "HE003" },
        { "TO_A", "HE004" },
        { "SV_A", "HE005" },
        { "HU_A", "HE006" },
        { "TY_A", "HE007" },
        { "TR_W", "HE008" },
        { "TR_A", "HE009" },
        { "TI_W", "HE010" },
        { "HI_A", "HE011" },
        { "TI_A", "HE012" },
        { "BZ_W", "HE013" },
        { "IS_W", "HE014" },
        { "WS_W", "HE015" },
        { "HW_W", "HE016" },
        { "LE_W", "HE017" },
        { "ZR_Y", "HE018" },
        { "CF_W", "HE019" },
        { "LS_W", "HE020" },
        { "WW_Y", "HE021" },
        { "LB_Y", "HE022" },
        { "LE_Y", "HE023" },
        { "BZ_A", "HE024" },
        { "WS_A", "HE025" },
        { "FF_A", "HE026" },
        { "FA_A", "HE027" },
        { "FA_Y", "HE028" },
        { "HW_A", "HE029" },
        { "LE_A", "HE030" },
        { "SU_W", "HE031" },
        { "LS_Y", "HE032" },
        { "CF_A", "HE033" },
        { "ZF_Y", "HE034" },
        { "FG_Y", "HE035" },
        { "SM_Y", "HE036" },
        { "EC_W", "HE037" },
        { "EH_W", "HE038" },
        { "HZ_W", "HE039" },
        { "FZ_W", "HE040" },
        { "HT_Y", "HE041" },
        { "WC_Y", "HE042" },
        { "FR_Y", "HE043" },
        { "EC_A", "HE044" },
        { "EH_A", "HE045" },
        { "HZ_A", "HE046" },
        { "DS_W", "HE047" },
        { "WI_Y", "HE048" },
        { "SU_Y", "HE049" },
        { "AS_Y", "HE050" },
        { "WC_W", "HE051" },
        { "FZ_A", "HE052" },
        { "WC_A", "HE053" },
        { "AF_W", "HE054" },
        { "AF_Y", "HE055" },
        { "DU_Y", "HE056" },
        { "LW_Y", "HE057" },
        { "LS_A", "HE058" },
        { "HF_W", "HE059" },
        { "SR_W", "HE060" },
        { "GL_W", "HE061" },
        { "HF_A", "HE062" },
        { "UP_W", "HE063" },
        { "SE_W", "HE064" },
        { "SR_A", "HE065" },
        { "GL_A", "HE066" },
        { "MF_Y", "HE067" },
        { "MS_Y", "HE068" },
        { "SC_Y", "HE069" },
        { "UP_Y", "HE070" },
        { "LO_Y", "HE071" },
        { "AF_V", "HE075" },
        { "UP_A", "HE076" },
        { "TAV_W", "HE077" },
        { "TAV_A", "HE078" },
        { "TO_W", "HE0110" }
    };
    
    /// <summary>
    /// Dictionary of state abbreviations converted into their full names.
    /// </summary>
    private static Dictionary<string, string> _states = new Dictionary<string, string>
    {
        { "AL", "Alabama" },
        { "AK", "Alaska" },
        { "AZ", "Arizona" },
        { "AR", "Arkansas" },
        { "CA", "California" },
        { "CO", "Colorado" },
        { "CT", "Connecticut" },
        { "DE", "Delaware" },
        { "FL", "Florida" },
        { "GA", "Georgia" },
        { "HI", "Hawaii" },
        { "ID", "Idaho" },
        { "IL", "Illinois" },
        { "IN", "Indiana" },
        { "IA", "Iowa" },
        { "KS", "Kansas" },
        { "KY", "Kentucky" },
        { "LA", "Louisiana" },
        { "ME", "Maine" },
        { "MD", "Maryland" },
        { "MA", "Massachusetts" },
        { "MI", "Michigan" },
        { "MN", "Minnesota" },
        { "MS", "Mississippi" },
        { "MO", "Missouri" },
        { "MT", "Montana" },
        { "NE", "Nebraska" },
        { "NV", "Nevada" },
        { "NH", "New Hampshire" },
        { "NJ", "New Jersey" },
        { "NM", "New Mexico" },
        { "NY", "New York" },
        { "NC", "North Carolina" },
        { "ND", "North Dakota" },
        { "OH", "Ohio" },
        { "OK", "Oklahoma" },
        { "OR", "Oregon" },
        { "PA", "Pennsylvania" },
        { "RI", "Rhode Island" },
        { "SC", "South Carolina" },
        { "SD", "South Dakota" },
        { "TN", "Tennessee" },
        { "TX", "Texas" },
        { "UT", "Utah" },
        { "VT", "Vermont" },
        { "VA", "Virginia" },
        { "WA", "Washington" },
        { "WV", "West Virginia" },
        { "WI", "Wisconsin" },
        { "WY", "Wyoming" }
    };


    /// <summary>
    /// Utility for matching event & severity codes to their corresponding vocal key
    /// </summary>
    /// <param name="vocalCheck">Event & severity check from the AlertDetails endpoint</param>
    /// <returns>The vocal code as a string</returns>
    private string? MapVocalCode(string vocalCheck)
    {
        try
        {
            string vocalKey = _vocalCodes[vocalCheck];
            return vocalKey;
        }
        catch (Exception ex)
        {
            Log.Error($"Vocal check failed for key {vocalCheck}.");
            Log.Debug(ex.Message);

            return null;
        }
    }

    public async Task<string?> MakeRecord(List<GenericResponse<AlertDetailResponse>> alertDetails)
    {
        string recordPath = Path.Combine(AppContext.BaseDirectory, "temp", "BERecord.xml");
        BERecordRoot root = new BERecordRoot();
        List<BERecord> alerts = new List<BERecord>();

        root.Type = "BERecord";
        root.BERecord = alerts;

        if (alertDetails.Count < 1)
        {
            await File.WriteAllTextAsync(recordPath, "<Data type=\"BERecord\"><BERecord></BERecord></Data>");
            return recordPath;
        }

        foreach (var details in alertDetails)
        {
            var detail = details.ParsedData.alertDetail;
            var locationInfo = details.Location;

            BERecord record = new BERecord();
            BEHdr header = new BEHdr();
            BEvent bEvent = new BEvent();
            BLocations locations = new BLocations();
            BStCd stateInfo = new BStCd();
            BEData data = new BEData();
            BHdln headline = new BHdln();
            BNarrTxt narrative = new BNarrTxt();
            
            // Timestamp parsing
            var endTime = DateTimeOffset.FromUnixTimeSeconds(detail.endTimeUTC).ToString("yyyy MM dd HH mm")
                .Replace(" ", "");
            var expireTime = DateTimeOffset.FromUnixTimeSeconds(detail.expireTimeUTC)
                .ToString("yyyy MM dd HH mm").Replace(" ", "");
            var issueTime = DateTime.Parse(detail.issueTimeLocal).ToString("yyyy MM dd HH mm").Replace(" ", "");
            var processTime = DateTimeOffset.FromUnixTimeSeconds(detail.processTimeUTC).ToString("yyyy MM dd HH mm")
                .Replace(" ", "");

            record.Id = 0000000;
            record.LocationKey =
                $"{detail.areaId}_{detail.phenomena}_{detail.significance}_{detail.eventTrackingNumber}_{detail.officeCode}";
            record.ClientKey = record.LocationKey;
            record.BEHdr = header;
            record.BEData = data;

            header.BPIL = detail.productIdentifier;
            header.BEvent = bEvent;
            header.BSgmntChksum = detail.identifier;
            header.ProcTm = processTime;
            
            EActionCd eActionCd = new EActionCd();
            eActionCd.EActionPriority = detail.messageTypeCode;

            switch (detail.messageType)
            {
                case "Update":
                    eActionCd.Text = "CON";
                    break;
                case "New":
                    eActionCd.Text = "NEW";
                    break;
            }
            
            bEvent.EActionCd = eActionCd;

            bEvent.EPhenom = detail.phenomena;
            bEvent.ESgnfcnc = detail.significance;
            bEvent.EETN = detail.eventTrackingNumber;
            bEvent.EDesc = detail.eventDescription;
            bEvent.EEndTmUTC = endTime;
            bEvent.ESvrty = detail.severityCode;
            bEvent.EExpTmUTC = expireTime;

            BLocCd loc = new BLocCd();

            loc.BLoc = detail.areaName;
            loc.BLocTyp = detail.areaTypeCode;
            loc.Text = detail.areaId;
            stateInfo.BSt = _states[locationInfo.stCd];
            stateInfo.Text = locationInfo.stCd;
            locations.BStCd = stateInfo;
            locations.BLocCd = loc;
            locations.BCntryCd = detail.countryCode;
            locations.BTzAbbrv = detail.effectiveTimeLocalTimeZone;

            header.BLocations = locations;

            data.BIssueTmUTC = issueTime;
            data.BHdln = headline;
            data.BNarrTxt = narrative;

            headline.BHdlnTxt = detail.headlineText;
            headline.BVocHdlnCd = MapVocalCode($"{detail.phenomena}_{detail.significance}");

            narrative.BNarrTxtLang = "en_US";
            narrative.BLn = detail.texts[0].description.Replace("\n", "");
            
            alerts.Add(record);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(BERecordRoot));
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        using (StreamWriter sw = new StreamWriter(recordPath))
        {
            serializer.Serialize(sw, root, ns);
            sw.Close();
        }

        return recordPath;
    }
    
}