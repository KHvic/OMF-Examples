using System;


    /// <summary>
    /// Sample data structure to represent data objects in the target system.
    /// </summary>
    class SimpleType
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
        
        public const string JsonSchema =
            @"{""id"": ""SimpleType"",""type"": ""object"",
                ""classification"": ""dynamic"",
                ""properties"": {
                    ""Time"": { ""type"": ""string"", ""format"": ""date-time"", ""isindex"": true },
                    ""Value"": { ""type"": ""number"", ""format"": ""float64"" }
                }
            }";
    }
    

