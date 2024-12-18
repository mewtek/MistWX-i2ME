using System.Collections.Generic;
using System.Text;

namespace TWC.Util.JobProcessor
{
    public class WorkRequest
    {
        public WorkRequest()
        {
            this.Parameters = new Dictionary<string, string>();
            this.PropertyBag = new Dictionary<string, string>();
        }

        public string JobName { get; set; }

        public bool IsParallel { get; set; }

        public Dictionary<string, string> Parameters { get; private set; }

        public Dictionary<string, string> PropertyBag { get; private set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.JobName);
            stringBuilder.Append('(');
            foreach (KeyValuePair<string, string> parameter in this.Parameters)
                stringBuilder.AppendFormat("{0}={1},", (object)parameter.Key, (object)parameter.Value);
            if (this.Parameters.Count > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }
    }
}
