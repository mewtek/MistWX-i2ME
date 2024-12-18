using System;
using System.Xml.Serialization;

namespace TWC.Util.DataProvider
{
  public interface IDataProvider : IDisposable
  {
    [XmlIgnore]
    uint Count { get; }

    [XmlIgnore]
    uint CountRemaining { get; }

    void Start();

    void GetNextBytes(ref byte[] buf, uint pos, uint count);
  }
}
