using System.Collections.Generic;


namespace tools
{

public class PairComparator<T, U> : IEqualityComparer<KeyValuePair<T, U>>
{
	public bool Equals (KeyValuePair<T, U> p1, KeyValuePair<T, U> p2)
	{
		//TODO3 check again that this Equals does value comparison for VectorInt3
		return p1.Key.Equals(p2.Key) && p1.Value.Equals(p2.Value);
	}
		
	public int GetHashCode(KeyValuePair<T, U> p)
	{
		return p.GetHashCode();
	}
	
}

}

