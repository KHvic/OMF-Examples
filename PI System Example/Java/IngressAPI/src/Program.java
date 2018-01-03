import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.TimeZone;
import javax.net.ssl.SSLContext;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.conn.ssl.SSLConnectionSocketFactory;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.*;
import org.apache.http.ssl.SSLContextBuilder;
import com.fasterxml.jackson.databind.*;

import Container.Container;
import StreamValues.DynamicStreamValues;
import StreamValues.StaticStreamValues;
import Type.DynamicType;
import Type.LinkType;
import Type.SourceTarget;
import Type.StaticType;

public class Program {

	/*CONFIGURATION*/
	static final String INGRESS_URL = "ENTER YOUR INGRESS URL";
	static final String PRODUCER_TOKEN = "ENTER PRODUCER TOKEN";
	
	static HttpClient client;
	static HttpPost httppost;
	
	public static void main(String[] args) throws Exception {
		
		Init();
		
		//Object to JSON Converter
		com.fasterxml.jackson.databind.ObjectWriter ow = new ObjectMapper().writer().withDefaultPrettyPrinter();
		
		/*START OF STATIC DATA EXAMPLE*/
		
		//CREATE TYPE
		httppost.setHeader("messagetype","type");
		StringEntity entity = new StringEntity(StaticType.JSONSCHEMA);
		httppost.setEntity(entity);
		SendRequest(httppost);
		
		//CREATE DATA
		httppost.setHeader("messagetype","data");
		List<StaticType> list = new ArrayList<StaticType>();
		for (int i = 1; i <4; i++)
			list.add(new StaticType(String.valueOf(i), "Element" + i, "A" + i));
		StaticStreamValues staticStream = new StaticStreamValues("StaticType", new ArrayList<Object>(list));
		
		//CREATE LINK
		LinkType link1 = new LinkType(new SourceTarget("StaticType","_ROOT"), new SourceTarget("StaticType","1"));
		LinkType link2 = new LinkType(new SourceTarget("StaticType","1"), new SourceTarget("StaticType","2"));
		LinkType link3 = new LinkType(new SourceTarget("StaticType","2"), new SourceTarget("StaticType","3"));
		
		List<LinkType> list2 = new ArrayList<LinkType>(){{
			add(link1);
			add(link2);
			add(link3);
		}};
		StaticStreamValues linkStream = new StaticStreamValues("__Link", new ArrayList<Object>(list2));
		
		List<Object> stream = new ArrayList<Object>() {{
			add(staticStream);
			add(linkStream);
		}};
		
		entity = new StringEntity(ow.writeValueAsString(stream));
		httppost.setEntity(entity);
		SendRequest(httppost);
		/*END OF STATIC DATA EXAMPLE*/
		
		/*START OF DYNAMIC DATA EXAMPLE*/

		//CREATE TYPE
		httppost.setHeader("messagetype","type");
		
		entity = new StringEntity(DynamicType.JSONSCHEMA);
		httppost.setEntity(entity);
		SendRequest(httppost);
		
		//CREATE CONTAINER
		httppost.setHeader("messagetype","container");
		
		Container c1 = new Container("TestStream1","DynamicType");
		Container c2 = new Container("TestStream2","DynamicType");
		entity = new StringEntity(ow.writeValueAsString(new Container[] {c1,c2}));
		httppost.setEntity(entity);
		SendRequest(httppost);
		
		//CREATE DATA
		httppost.setHeader("messagetype","data");
		
		// Loop indefinitely, sending time series to two streams periodically.
		while(true) 
		{
			List<DynamicType> values = new ArrayList<DynamicType>();
			for (int i = 0; i <5; i++) 
			{
				DateFormat df = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SS'Z'");
				df.setTimeZone(TimeZone.getTimeZone("UTC"));
				
				values.add(new DynamicType(df.format(new Date()), i));
				Thread.sleep(10);
			}
			
			DynamicStreamValues val1 = new DynamicStreamValues(c1.Id,new ArrayList<Object>(values));
			DynamicStreamValues val2 = new DynamicStreamValues(c2.Id,new ArrayList<Object>(values));
			entity = new StringEntity(ow.writeValueAsString(new DynamicStreamValues[] {val1,val2}));
			
			httppost.setEntity(entity);
			SendRequest(httppost);
			Thread.sleep(1000);
		}
		
		/*END OF DYNAMIC DATA EXAMPLE*/
	}
	
	// Execute post request
	public static void SendRequest(HttpPost httppost) throws Exception {
		HttpResponse response = client.execute(httppost);
		
		BufferedReader rd = new BufferedReader(new InputStreamReader(
                response.getEntity().getContent()));
        String line = "";
        while ((line = rd.readLine()) != null) {
            System.out.println(line);
        }
	}

	// Initialize httppost, SSL settings, and httpclient
	public static void Init() throws Exception{
		//SSL Connection
		SSLContextBuilder sslContextBuilder = SSLContextBuilder.create();
		sslContextBuilder.loadTrustMaterial(new org.apache.http.conn.ssl.TrustSelfSignedStrategy());
		SSLContext sslContext = sslContextBuilder.build();
		org.apache.http.conn.ssl.SSLConnectionSocketFactory sslSocketFactory =
		        new SSLConnectionSocketFactory(sslContext, new org.apache.http.conn.ssl.DefaultHostnameVerifier());
		HttpClientBuilder httpClientBuilder = HttpClients.custom().setSSLSocketFactory(sslSocketFactory);
		
		//Init client
		client = httpClientBuilder.build();
		httppost = new HttpPost(INGRESS_URL);
		
		/*SET HEADER*/
		httppost.setHeader("producertoken",PRODUCER_TOKEN);
		httppost.setHeader("action","create");
		httppost.setHeader("messageformat","json");
		httppost.setHeader("omfversion","1.0");
	}
}
