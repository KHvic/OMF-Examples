package Type;

public class StaticType {

	public String Id;
	public String Name;
	public String Model;
	public static final String JSONSCHEMA =  
			"[{\"id\": \"StaticType\",\"type\": \"object\",\"classification\": \"static\", "
			+ "\"properties\": {\"Id\": { \"type\": \"string\", \"isindex\": true },"
			+ "\"Name\": { \"type\": \"string\", \"isname\": true},"
			+ "\"Model\": { \"type\": \"string\"}} }]";
	
	public StaticType(String Id, String Name, String Model) {
		this.Id = Id;
		this.Name = Name;
		this.Model = Model;
	}
	
}
