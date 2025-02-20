using System.Collections.Generic;


namespace tools
{

public class Vec3Hash : IEqualityComparer<VectorInt3>
{
	//TODO: use some real hash function here
	static int hasher(short value)
	{
		return value;
	}
		
	public int GetHashCode(VectorInt3 v)
	{		
		long seed = hasher(v.x);
		seed = 0x9e3779b9 + hasher(v.y) + (seed << 6) + (seed>>2);
		seed = 0x9e3779b9 + hasher(v.z) + (seed << 6) + (seed>>2);
			
//		return v.x + v.y + v.z;
		return (int)seed;
	}
		
	public bool Equals(VectorInt3 v1, VectorInt3 v2)
	{
		return GetHashCode(v1) == GetHashCode(v2);
	}
}

}
