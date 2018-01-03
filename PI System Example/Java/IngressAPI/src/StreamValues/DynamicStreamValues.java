package StreamValues;
import java.util.List;

public class DynamicStreamValues {
	public String ContainerId;
	public List<Object> values;
	
	public DynamicStreamValues(String c, List<Object> val) {
		ContainerId = c;
		values = val;
	}
}
