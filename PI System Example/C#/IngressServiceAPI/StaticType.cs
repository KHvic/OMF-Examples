
using System;
namespace IngressServiceAPI
{
    public class StaticType
    {

        public String Id { get; set; }
        public String Name { get; set; }
        public String Model { get; set; }
        public const string JsonSchema =
            @"{""id"": ""StaticType"",""type"": ""object"",
                ""classification"": ""static"",
                ""properties"": {
                    ""Id"": { ""type"": ""string"", ""isindex"": true },
                    ""Name"": { ""type"": ""string"", ""isname"": true},
                    ""Model"": { ""type"": ""string""}
                }
            }";
    }
}

