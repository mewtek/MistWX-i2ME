using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TWC.Util.JobProcessor;

namespace TWC.I2.MsgEncode.ProcessingSteps
{
    public class ExecMsgEncodeStep : IMsgEncodeStep
    {
        private readonly Regex workReqRegex = new Regex("^([^(]+)(\\(([^)]*)\\))?$");
        private readonly Regex argRegex = new Regex("^([^=]+)=(.+)$");
        private string workRequest;
        private Dictionary<string, string> parameters;

        public ExecMsgEncodeStep(string workRequest)
        {
            WorkRequest workRequest1 = this.MakeFromReqString(workRequest);
            this.workRequest = workRequest1.JobName;
            this.parameters = workRequest1.Parameters;
        }

        public ExecMsgEncodeStep(string workRequest, IDictionary<string, string> arg_ps)
        {
            WorkRequest workRequest1 = this.MakeFromReqString(workRequest);
            this.workRequest = workRequest1.JobName;
            this.parameters = workRequest1.Parameters;
            if (arg_ps == null)
                return;
            foreach (KeyValuePair<string, string> argP in (IEnumerable<KeyValuePair<string, string>>)arg_ps)
                this.parameters.Add(argP.Key, argP.Value);
        }

        public string Tag
        {
            get
            {
                return "Exec";
            }
        }

        public string Encode(string payloadFile, XmlElement descriptor)
        {
            StringBuilder stringBuilder = new StringBuilder(this.workRequest);
            stringBuilder.Append("(File={0}");
            foreach (KeyValuePair<string, string> parameter in this.parameters)
                stringBuilder.Append(string.Format(",{0}={1}", (object)parameter.Key, (object)parameter.Value));
            stringBuilder.Append(")");
            descriptor.SetAttribute("workRequest", stringBuilder.ToString());
            return payloadFile;
        }

        private WorkRequest MakeFromReqString(string workReqStr)
        {
            if (string.IsNullOrEmpty(workReqStr))
                return (WorkRequest)null;
            Match match = this.workReqRegex.Match(workReqStr);
            if (!match.Groups[0].Success)
            {
                //Log.Error("malformed work request string: '{0}'", (object)workReqStr);
                return (WorkRequest)null;
            }
            WorkRequest wreq = new WorkRequest()
            {
                JobName = match.Groups[1].Value.Trim()
            };
            wreq.JobName = wreq.JobName.Replace('/', '\\');
            if (match.Groups[3].Success && !this.ParseWorkRequestArgs(wreq, workReqStr, match.Groups[3].Value))
                wreq = (WorkRequest)null;
            return wreq;
        }

        private bool ParseWorkRequestArgs(WorkRequest wreq, string wrStr, string argsStr)
        {
            bool flag = true;
            string str = argsStr;
            char[] chArray = new char[1] { ',' };
            foreach (string argStr in str.Split(chArray))
            {
                if (!this.ParseWorkRequestArg(wreq, wrStr, argStr))
                    flag = false;
            }
            return flag;
        }

        private bool ParseWorkRequestArg(WorkRequest wreq, string wrStr, string argStr)
        {
            if (string.IsNullOrEmpty(argStr))
                return true;
            Match match = this.argRegex.Match(argStr);
            if (!match.Groups[0].Success)
            {
                //Log.Error("malformed arg '{0}' in work request '{1}'", (object)argStr, (object)wrStr);
                return false;
            }
            string str = match.Groups[2].Value.Trim();
            if (str.StartsWith("\"") && str.EndsWith("\""))
                str = str.Substring(1, str.Length - 2);
            wreq.Parameters[match.Groups[1].Value.Trim()] = str;
            return true;
        }
    }
}
