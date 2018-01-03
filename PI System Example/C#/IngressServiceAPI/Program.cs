using IngressServiceAPI.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;

namespace IngressServiceAPI
{
    class Program
    {
        static void Main(string[] args)
        {   

            // Set this up in your app.config
            string ingressServiceUrl = ConfigurationManager.AppSettings["IngressServiceUrl"];
            string producerToken = ConfigurationManager.AppSettings["ProducerToken"];

            IngressClient client = new IngressClient(ingressServiceUrl, producerToken);

            // Use compression when sending data.  For such small sample messages, compression doesn't 
            // save us much space, but we're doing it here for demonstration sake.
            client.UseCompression = false;


            // Types can be classified under two different category:
            // Dynamic Type: Changes frequently (Temperature/Oil Level)
            // Static Type: Do not change frequently (Name)

            // 1) Send the Types message - DynamicType is a dynamic type
            client.CreateTypes(new string[] { DynamicType.JsonSchema });

            // 2) Send the Containers message - Container for dynamic type
            ContainerInfo stream1 = new ContainerInfo() { Id = "TestStream1", TypeId = "DynamicType" };
            ContainerInfo stream2 = new ContainerInfo() { Id = "TestStream2", TypeId = "DynamicType" };
            
            client.CreateContainers(new ContainerInfo[] { stream1, stream2});

            // Send the Types messages for static type
            client.CreateTypes(new string[] { StaticType.JsonSchema });

            // Create couple of Static Type Data named Element
            List<StaticType> list = new List<StaticType>();
            for (int i = 1; i < 4; i++)
                list.Add(new StaticType() { Id = i.ToString(), Name = "Element" + i, Model = "A" + i });

            StaticStreamValues staticStream = new StaticStreamValues(){ TypeId = "StaticType",Values = list};

            //  __Link Type to link static data
            //  Source is the parent, Target is the child
            //  Head element need to be linked to _ROOT
            LinkType link1 = new LinkType()
            { Source = new SourceTarget() { typeid = "StaticType", index = "_ROOT" },
              Target = new SourceTarget() { typeid = "StaticType", index = "1"}
            };
            LinkType link2 = new LinkType()
            {
                Source = new SourceTarget() { typeid = "StaticType", index = "1" },
                Target = new SourceTarget() { typeid = "StaticType", index = "2" }
            };
            LinkType link3 = new LinkType()
            {
                Source = new SourceTarget() { typeid = "StaticType", index = "2" },
                Target = new SourceTarget() { typeid = "StaticType", index = "3" }
            };
            List<LinkType> list2 = new List<LinkType>{ link1, link2, link3 };

            // Send the static data
            StaticStreamValues linkStream = new StaticStreamValues() { TypeId = "__Link", Values = list2 };


            client.SendValuesAsync2(new StaticStreamValues[] { staticStream, linkStream }).Wait();

            // Here we loop indefinitely, sending 10 time series events to two streams every second.
            while (true)
            {
                // Create our set of values to send to our streams
                List<DynamicType> values = new List<DynamicType>();
                for(int i = 0; i < 10; i++)
                {
                    values.Add(new DynamicType() { Time = DateTime.UtcNow, Value = i });
                    Thread.Sleep(10);  // Offset the time-stamps by 10 ms
                }
                Console.Write("Running\n");
                DynamicStreamValues vals1 = new DynamicStreamValues() { ContainerId = stream1.Id, Values = values };
                DynamicStreamValues vals2 = new DynamicStreamValues() { ContainerId = stream2.Id, Values = values };

                // Now send them
                client.SendValuesAsync(new DynamicStreamValues[] { vals1, vals2 }).Wait();

                Thread.Sleep(1000);
            }
        }
    }
}