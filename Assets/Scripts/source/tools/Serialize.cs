using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;

namespace tools
{

public static partial class Tools
{
		
//	// Write an object of arbitrary (serializeable) type to an output stream
//	public static FileStream Serialize<T>(FileStream s, T v) where T : ISerializable
//	{
//		BinaryFormatter bf = new BinaryFormatter();
//	  	bf.Serialize(s, v);
//			
//	  	return s;
//	}
//		
//	public static FileStream Serialize<T>(FileStream s, T v, BinaryFormatter bf) where T : ISerializable
//	{
//	  	bf.Serialize(s, v);
//			
//	  	return s;
//	}
//	
//
//	// Read an object of arbitrary (serializeable) type from an input stream
//	public static FileStream Deserialize<T>(FileStream s, T v)
//	{	
//		BinaryFormatter bf = new BinaryFormatter();
//		v = (T)bf.Deserialize(s);
//			
//		return s;
//	}
//		
//	public static FileStream Deserialize<T>(FileStream s, T v, BinaryFormatter bf)
//	{	
//		v = (T)bf.Deserialize(s);
//			
//		return s;
//	}
//		
		
		
		
//	//std::ostream operator <<(std::ostream s, PieceDescriptor pd);
//	public static FileStream Serialize<T>(FileStream s, T v) where T : ISerializable
//	{
//		BinaryWriter bw = new BinaryWriter(s);
//		bw.Write(v);
//			
//	  	return s;
//	}
//
//	// Read an object of arbitrary (serializeable) type from input stream
//	public static FileStream Deserialize<T>(FileStream s, ref T v) where T : int, bool, float, short, double
//	{
//		BinaryReader br = new BinaryReader(s);
//		br.Read(v);
//			
//	  	return s;
//	}
		
	public static void Serialize(BinaryWriter bw, VectorInt3 v)
	{
		bw.Write((int)v.x);
		bw.Write((int)v.y);
		bw.Write((int)v.z);
	}
		
	public static void Deserialize(BinaryReader bw, out VectorInt3 v)
	{
		v = new VectorInt3();
		v.x = (short)bw.ReadInt32();
		v.y = (short)bw.ReadInt32();
		v.z = (short)bw.ReadInt32();
	}	
		
		
	public static void Serialize(BinaryWriter bw, Vector3 v)
	{
		bw.Write((float)v.x);
		bw.Write((float)v.y);
		bw.Write((float)v.z);
	}
		
	public static void Deserialize(BinaryReader bw, out Vector3 v)
	{
		v = new Vector3();
		v.x = (float)bw.ReadSingle();
		v.y = (float)bw.ReadSingle();
		v.z = (float)bw.ReadSingle();
	}	
		
		
	public static void Serialize(BinaryWriter bw, Vector4 v)
	{
		bw.Write((float)v.x);
		bw.Write((float)v.y);
		bw.Write((float)v.z);
		bw.Write((float)v.w);
	}
		
	public static void Deserialize(BinaryReader bw, out Vector4 v)
	{
		v = new Vector4();
		v.x = (float)bw.ReadSingle();
		v.y = (float)bw.ReadSingle();
		v.z = (float)bw.ReadSingle();
		v.w = (float)bw.ReadSingle();
	}	
		
}
	
}