using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using pcs;


public class CircuitRenderer
{
	protected static Dictionary<PieceType, List<Vector3>> m_wireframes;
	
	public static HashSet<uint> m_unstable;
	
	public const float s_durationGlow = 1.0f;
	protected static float s_time = 0.0f;
	public static float t { get { return s_time; } }
	
	//
	//
	
	public static List<Vector3> GetWireframe(PieceType type, MeshFilter meshFilter)
	{
		if (CircuitRenderer.m_wireframes == null) {
			CircuitRenderer.m_wireframes = new Dictionary<PieceType, List<Vector3>>();
		}
		if (!CircuitRenderer.m_wireframes.ContainsKey(type)){
			if(!GenerateWireframe(type, meshFilter)){
				return null;
			}
		}
		
		return CircuitRenderer.m_wireframes[type];	
	}
	
	protected static bool GenerateWireframe(PieceType type, MeshFilter meshFilter)
	{
		var lines = new List<Vector3>();
		if (meshFilter != null){
		    var mesh = meshFilter.mesh;
		    var vertices = mesh.vertices;
		    var triangles = mesh.triangles;
		   
		    for (int i = 0; i < triangles.Length / 3; i++){
		        lines.Add(vertices[triangles[i * 3]]);
		        lines.Add(vertices[triangles[i * 3 + 1]]);
		        lines.Add(vertices[triangles[i * 3 + 2]]);
		    }
			CircuitRenderer.m_wireframes[type] = lines;
			return true;
		} else {
			return false;
		}
	}

	//
	//
	
	public CircuitRenderer()
	{
	}
	
	public static void Update(float seconds)
	{
		s_time += (seconds / s_durationGlow);
	}

	//
	//
	
	public static bool AreDisjoint<T>(HashSet<T> set1, HashSet<T> set2)
	{
	  	if (set1.Count == 0 || set2.Count == 0){
			return true;
		}
		
		return !set1.Overlaps(set2);
	}
	
	
	public void UpdateVisual(Circuit circuit)
	{
		if (m_unstable == null) {
			 m_unstable = new HashSet<uint>();
		}
		m_unstable.Clear();
		
		var unstablePairs = circuit.GetUnstablePairs();
		foreach (var usp in unstablePairs){
			m_unstable.Add(usp.Key);
			m_unstable.Add(usp.Value);
		}
	}
	

}
	