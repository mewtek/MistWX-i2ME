using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TWC.Util.JobProcessor
{
    public class WorkRequestFactory
    {
        private static readonly Regex workReqRegex = new Regex("^([^(]+)(\\(([^)]*)\\))?$");
        private static readonly Regex argRegex = new Regex("^([^=]+)=(.+)$");

        public static List<WorkRequest> MakeListFromReqString(
          string workReq,
          bool isParallelJob,
          Dictionary<string, string> propertyBag)
        {
            string[] strArray = workReq.Split(';');
            List<WorkRequest> workRequestList = new List<WorkRequest>();
            foreach (string workReqStr in strArray)
            {
                if (!string.IsNullOrEmpty(workReqStr))
                {
                    WorkRequest workRequest = WorkRequestFactory.MakeFromReqString(workReqStr, isParallelJob, propertyBag);
                    if (workRequest != null)
                        workRequestList.Add(workRequest);
                }
            }
            return workRequestList;
        }

        public static WorkRequest MakeFromReqString(
          string workReqStr,
          bool isParallelJob,
          Dictionary<string, string> propertyBag)
        {
            if (string.IsNullOrEmpty(workReqStr))
                return (WorkRequest)null;
            Match match = WorkRequestFactory.workReqRegex.Match(workReqStr);
            if (!match.Groups[0].Success)
            {
                //Log.Error("malformed work request string: '{0}'", (object)workReqStr);
                return (WorkRequest)null;
            }
            WorkRequest wreq = new WorkRequest()
            {
                JobName = match.Groups[1].Value.Trim().Replace('/', '\\'),
                IsParallel = isParallelJob
            };
            if (propertyBag != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in propertyBag)
                    wreq.PropertyBag.Add(keyValuePair.Key, keyValuePair.Value);
            }
            if (match.Groups[3].Success && !WorkRequestFactory.ParseWorkRequestArgs(wreq, workReqStr, match.Groups[3].Value))
                wreq = (WorkRequest)null;
            return wreq;
        }

        public static WorkRequest Make(
          string jobName,
          string[] parameterNames,
          string[] parameterValues,
          bool isParallelJob)
        {
            if (parameterValues.Length != parameterNames.Length)
            {
                //Log.Error("Parameter names and value arrays don't match in length for job '{0}'", (object)jobName);
                return (WorkRequest)null;
            }
            WorkRequest workRequest = new WorkRequest()
            {
                JobName = jobName,
                IsParallel = isParallelJob
            };
            for (int index = 0; index < parameterNames.Length; ++index)
                workRequest.Parameters[parameterNames[index]] = parameterValues[index];
            return workRequest;
        }

        private static bool ParseWorkRequestArgs(WorkRequest wreq, string wrStr, string argsStr)
        {
            return ((IEnumerable<string>)argsStr.Split(',')).Aggregate<string, bool>(true, (Func<bool, string, bool>)((current, arg) => current & WorkRequestFactory.ParseWorkRequestArg(wreq, wrStr, arg)));
        }

        private static bool ParseWorkRequestArg(WorkRequest wreq, string wrStr, string argStr)
        {
            if (!string.IsNullOrEmpty(argStr))
            {
                Match match = WorkRequestFactory.argRegex.Match(argStr);
                if (!match.Groups[0].Success)
                {
                    //Log.Warning("malformed arg '{0}' in work request '{1}' - ignoring request", (object)argStr, (object)wrStr);
                    return false;
                }
                string str = match.Groups[2].Value.Trim();
                if (str.StartsWith("\"") && str.EndsWith("\""))
                    str = str.Substring(1, str.Length - 2);
                wreq.Parameters[match.Groups[1].Value.Trim()] = str;
            }
            return true;
        }
    }
}
