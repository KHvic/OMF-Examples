namespace IngressServiceAPI.API
{
    /// <summary>
    /// This defines the relationship between a container and a type. Id and TypeId
    /// property names are defined in the OMF spec.  This class is serialized into 
    /// an OMF message.
    /// </summary>
    public class ContainerInfo
    {
        public string Id { get; set; }
        public string TypeId { get; set; }
    }
}