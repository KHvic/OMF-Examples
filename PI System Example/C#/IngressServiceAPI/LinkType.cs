
using System;
namespace IngressServiceAPI
{
    public class SourceTarget {
        public string typeid { get; set; }
        public string index { get; set; }
    }
    public class LinkType
    {

        public SourceTarget Source { get; set; }
        public SourceTarget Target { get; set; }
        /* Already defined
        public const string JsonSchema =
            @"";*/
    }
}

