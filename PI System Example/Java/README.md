
# Libraries Required

- jackson-databind : https://github.com/FasterXML/jackson-databind/wiki/Downloads
- jackson-core : https://github.com/FasterXML/jackson-core/wiki
- jackson-annotation : https://github.com/FasterXML/jackson-annotations/wiki
- Apache HttpClient : https://hc.apache.org/downloads.cgi

# Purpose of libraries

- jackson-databind, jackson-core, jackson-annotation allows the conversion of an class object to JSON format.
- Apache HttpClient simplify the sending of post request.

# How to add libraries to project (Eclipse)
1. Download the required JAR files
2. Right click on Package Explorer -> Build Path -> Configure Build Path -> Click on Libraries Tab -> Add External JAR
3. Add all the required JAR files

# How to convert a class object to JSON
    
    //Object to JSON Converter
		com.fasterxml.jackson.databind.ObjectWriter ow = new ObjectMapper().writer().withDefaultPrettyPrinter();
    
    //This converts a class object into a JSON string, allowing a object oriented implementation for OMF
    String msg = ow.writeValueAsString(obj)

# How to send a post request
A function using Apache HttpClient has been defined to send http post request easily. To send a post request, use the SendRequest function as follows
  	
	//httppost is a pre-defined object based on the library defined 'HttpPost' class
  	StringEntity  new StringEntity(msg);
  	httppost.setEntity(entity);
  
 	//set the corresponding header for post request
  	httppost.setHeader("messagetype","data");
  
  	//This will send the post request
  	SendRequest(httppost);
