using System.Collections.Generic;

namespace IngressServiceAPI.API
{
    /// <summary>
    /// Holds a set of values for a stream.  The property names ContainerId and 
    /// Values are defined in the OMF spec.  This class is serialized into an 
    /// OMF message.
    /// </summary>
    public class StaticStreamValues
    {
        public string TypeId { get; set; }
        public IEnumerable<object> Values { get; set; }
    }
}