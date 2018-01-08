
Ingress Service API
===================

The Ingress Service API is a set of helper classes and methods used to send stream based data to a service endpoint that supports the OSIsoft Messaging Format (OMF).  

Currently, the API supports three operations:

- Creating types
- Creating containers
- Sending values

These operations are exposed via the ``IngressClient`` which is responsible for formatting the stream data into OMF messages, authenticating with the Ingress Service, and sending the data to the service endpoint.

Creating Dynamic Types
--------------
Types define the structure of the data that will be sent to the service.  We use `JSONSchema <http://json-schema.org/examples.html>`_ to describe types.  Types must be created and sent to the service before any streams associated with the type can be sent.  Here's an example of a type that contains an indexed date-time and a numeric value.  We give it an id of "DynamicType" which we then reference later on when creating streams.
::

    string typeSchema =
    @"{
          ""id"": ""DynamicType"",
          ""type"": ""object"",
		  ""classification"": ""dynamic"",
          ""properties"": {
              ""Time"": { ""type"": ""string"", ""format"": ""date-time"", ""isindex"": true },
              ""Value"": { ""type"": ""number"" }
          }
      }";

    ingressClient.CreateTypes(new string[] { typeSchema });

Creating Containers
----------------
Containers define a group of typed and ordered data.  Conceptually,  you could think of a container as relating to a sensor or a device that emits a continuous stream of data.   Since all data stored in the OSIsoft Cloud Services Data Store is associated with a stream, this needs to be created before data can be successfully sent and stored in the historian.

Containers are created by building a ``ContainerInfo`` object and passing it to the ``IngressClient``.  The ``ContainerInfo`` is a tuple that consist of a containerId and it's associated typeId.  The container Id can be any unique string that you want to use to identify your stream.
::

    ContainerInfo stream1 = new ContainerInfo() { Id = "TestStream1", TypeId = "DynamicType" };

    ingressClient.CreateContainers(new ContainerInfo[] { stream1 });


Sending Values
---------------
Once your type and container have been created, you can now send values associated with either the type or container. Since the OSIsoft Cloud Services Data Store currently only supports stream data, values are sent by creating a collection of ``StreamValues`` objects and then passing that to the ``IngressClient``.  ``StreamValues`` is an object that contains your container Id associated with your values, and a collection of objects that contain your values.
::

    // Create a collection of values and add your data
    var values = new List<StaticType>();
    values.Add(new DynamicType() { Time = t1, Value = 3.14 });
    values.Add(new DynamicType() { Time = t2, Value = 6.67384 });
    values.Add(new DynamicType() { Time = t3, Value = 299792458 });

    // Package them into a StreamValues object
    var vals1 = new DynamicStreamValues() { ContainerId = stream1.StreamId, Values = values };

    // Send them to the service
    ingressClient.SendValuesAsync(new StreamValues[] { vals1 });

Static Types
---------------
Static type can be defined similarly as the Dynamic type, with the classification's value set to "static". As compared to dynamic types which are defined for values that changes constantly such as temperature and oil level (PI Point), static types are more suitable for creating elements in the PI System.
::
        public const string JsonSchema =
            @"{""id"": ""StaticType"",""type"": ""object"",
                ""classification"": ""static"",
                ""properties"": {
                    ""Id"": { ""type"": ""string"", ""isindex"": true },
                    ""Name"": { ""type"": ""string"", ""isname"": true},
                    ""Model"": { ""type"": ""string""}
                }
            }";
	    
	ingressClient.CreateTypes(new string[] { JsonSchema });
	
Creating Elements
---------------
Elements can be created similarly as the Dynamic types according to the following examples which create 3 elements with id of 1, 2, and 3.
::

         // Create couple of Static Type Data named Element
         List<StaticType> list = new List<StaticType>();
         for (int i = 1; i < 4; i++)
              list.Add(new StaticType() { Id = i.ToString(), Name = "Element" + i, Model = "A" + i });

         StaticStreamValues staticStream = new StaticStreamValues(){ TypeId = "StaticType",Values = list};

Linking Elements
---------------
After elements have been created, they can be linked using the defined helper class 'LinkType', where the Source is the Parent element, and the Target is the child element. The index corresponds to the 'Id' of the created element.
::
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

            // Send the static data and link
            StaticStreamValues linkStream = new StaticStreamValues() { TypeId = "__Link", Values = list2 };
            client.SendValuesAsync2(new StaticStreamValues[] { staticStream, linkStream }).Wait();

OSIsoft Cloud Service (OCS) Example
------------------------
This example was extended from the existing example for OCS that can found at https://github.com/osisoft/OMF-Samples
