using UnityEngine;

/**
 * 
 */
public struct VectorInt2
{
	public static VectorInt2 zero = new VectorInt2(0, 0);
	
	public short x, y;
	
	public VectorInt2 (int xx, int yy)
	{
		x = (short)xx;
		y = (short)yy;
	}
	
	public VectorInt2 (Vector3 v)
	{
		x = (short) v.x;	
		y = (short) v.y;
	}
     
	public void copyTo (out Vector2 V)
	{
		V.x = x;
		V.y = y;
	}

	public Vector2 ToVector2 ()
	{
		return new Vector2 (x, y);
	}
	
	public static VectorInt2 operator- (VectorInt2 v) 
	{
		return new VectorInt2(-v.x, -v.y);
	}
	
	public static VectorInt2 operator- (VectorInt2 v, VectorInt2 other) 
	{
		return new VectorInt2(v.x - other.x, v.y - other.y);
	}
	
	public static VectorInt2 operator+ (VectorInt2 v, VectorInt2 other) 
	{
		return new VectorInt2(v.x + other.x, v.y + other.y);
	}
	
	public static VectorInt2 operator* (VectorInt2 v, int factor) 
	{
		return new VectorInt2(v.x * factor, v.y * factor);
	}
	
	public static VectorInt2 operator* (int factor, VectorInt2 v) 
	{
		return new VectorInt2(v.x * factor, v.y * factor);
	}
	
	public static VectorInt2 operator/ (VectorInt2 v, int divident) 
	{
		return new VectorInt2(v.x / divident, v.y / divident);
	}
	
	public int LengthAccum()
	{
		return Mathf.Abs(x) + Mathf.Abs(y);
	}
	
}

/**
 * 
 */
public struct VectorInt3
{
	public static VectorInt3 zero = new VectorInt3(0, 0, 0);
	
	public short x, y, z;
	
	public VectorInt3 (int xx, int yy, int zz)
	{
		x = (short)xx;
		y = (short)yy;
		z = (short)zz;
	}
	
	public VectorInt3 (Vector3 v)
	{
		x = (short) v.x;	
		y = (short) v.y;
		z = (short) v.z;
	}
	
	public VectorInt3 (int i)
	{
		x = (short) i;	
		y = (short) i;
		z = (short) i;
	}
     
	public void copyTo (out Vector3 V)
	{
		V.x = x;
		V.y = y;
		V.z = z;
	}

	public Vector3 ToVector3 ()
	{
		return new Vector3(x, y, z);
	}
	
	public static VectorInt3 operator- (VectorInt3 v) 
	{
		return new VectorInt3(-v.x, -v.y, -v.z);
	}
	
	public static VectorInt3 operator+ (VectorInt3 v, VectorInt3 other) 
	{
		return new VectorInt3(v.x + other.x, v.y + other.y, v.z + other.z);
	}
	
	public static VectorInt3 operator- (VectorInt3 v, VectorInt3 other) 
	{
		return new VectorInt3(v.x - other.x, v.y - other.y, v.z - other.z);
	}
	
	public static VectorInt3 operator* (VectorInt3 v, int factor) 
	{
		return new VectorInt3(v.x * factor, v.y * factor, v.z * factor);
	}
	
	public static VectorInt3 operator* (int factor, VectorInt3 v) 
	{
		return new VectorInt3(v.x * factor, v.y * factor, v.z * factor);
	}
	
	public static VectorInt3 operator/ (VectorInt3 v, int divident) 
	{
		return new VectorInt3(v.x / divident, v.y / divident, v.z / divident);
	}
	
	public VectorInt3 shiftNFloor (uint shift)
	{	
		return new VectorInt3((int)System.Math.Floor(((float)this.x) * 0.5f), 
			(int)System.Math.Floor(((float)this.y) * 0.5f), 
			(int)System.Math.Floor(((float)this.z) * 0.5f));
	}
	
	public int this[int index] {
		get {
			switch(index){
				case 0 : return x;
				case 1 : return y;
				case 2 : return z;
				default:
					throw new System.IndexOutOfRangeException();
			}
		}
		set {
		
			switch(index){
				case 0 : { x = (short)value; break; }
				case 1 : { y = (short)value; break; }
				case 2 : { z = (short)value; break; }
				default:
					throw new System.IndexOutOfRangeException();
			}
		}
	}
	
	
	
	public static bool operator== (VectorInt3 v1, VectorInt3 v2) 
	{
		return v1.x == v2.x && 
				v1.y == v2.y && 
				v1.z == v2.z;
	}
	
	public static bool operator!= (VectorInt3 v1, VectorInt3 v2) 
	{
		return v1.x  != v2.x || 
				v1.y != v2.y || 
				v1.z != v2.z;
	}
	
	public override bool Equals(object ob) 
	{
		if (ob is VectorInt3){
			VectorInt3 other = (VectorInt3)ob;
			return this.x == other.x && 
					this.y == other.y && 
					this.z == other.z;
		} else {
			return false;
		}
	}
	
	public override int GetHashCode()
	{
		int hx = this.x.GetHashCode();
		int hy = this.y.GetHashCode();
		int hz = this.z.GetHashCode();
//		
		return (this.x.GetHashCode() ^ this.y.GetHashCode()) ^ this.z.GetHashCode();
	}
	
	
	public new string ToString()
	{
		return "["+this.x+" , "+this.y+", "+this.z+"]";
	}
	
	public int LengthAccum()
	{
		return Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
	}

}

