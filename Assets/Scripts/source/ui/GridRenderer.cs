using UnityEngine;
using System;
using System.Collections.Generic;


/**
 * 	Singleton class
 * */
public class GridRenderer
{
	protected static GridRenderer s_instance;
	public static GridRenderer Instance {
		get { return s_instance; }
	}

	//
	//
		
	protected GameObject m_visual;
	protected UnityEngine.Material m_matGrid;

	protected Vector3 m_min = Vector3.zero;
	protected Vector3 m_max = Vector3.zero;

	//
	//
	
	public GridRenderer()
	{
		if (GridRenderer.s_instance == null){
			s_instance = this;
		} else {
			System.Diagnostics.Debug.Assert(false);
		}
	}
	
	//
	//

	public void InitVisual()
	{
		// Init mesh GameObject
			
		m_visual = new GameObject("grid");
		m_matGrid = new Material((Shader)Resources.Load("Shaders/Grid", typeof(Shader)));
		
		GridCS gridCS = m_visual.AddComponent<GridCS>();
		gridCS.gridRenderer = this;
	}
	
	public void UninitVisual()
	{
		GameObject.Destroy(m_visual);
		m_visual = null;
	}
	

	public void SetCircuitBB(Vector3 min, Vector3 max)
	{
		m_min = min;
		m_max = max;
		
		Vector3 size = m_max - m_min;
		Vector3 extend = new Vector3(Math.Max(50 - size.x, 0)/2, Math.Max(50 - size.y, 0)/2, Math.Max(50 - size.z, 0)/2);
		m_min -= extend;
		m_max += extend; 
		
		m_matGrid.SetFloat("_RadiusMaxGrid", 20.0f);
	}

	public void Draw()
	{
		// set shader
//		Vector4 center = (m_min + m_max) / 2.0f;
		Vector4 center = Camera.main.GetComponent<MouseOrbitZoom>().target.position;
		center.w = 1.0f;
		m_matGrid.SetVector("_PosFocus", Camera.mainCamera.worldToCameraMatrix * center);
		m_matGrid.SetPass(0);
		
		DrawLineSet(tools.Axis.X);
		DrawLineSet(tools.Axis.Y);
		DrawLineSet(tools.Axis.Z);

	}
	
	protected void DrawLineSet(tools.Axis axis)
	{
		Vector3 size = m_max - m_min;
		Vector4 v0 = new Vector4(0, 0, 0, 1); 
		Vector4 v1 = new Vector4(0, 0, 0, 1);
		
		
		GL.Begin(GL.LINES);
		
		if (axis == tools.Axis.X){
			v0.x = m_min.x;
			v1.x = m_max.x;
			
		  	for (int i = 0; i < size.y; i += 5){
				v0.y = v1.y = m_min.y + i;
				for (int j = 0; j < size.z; j += 5){
					v0.z = v1.z = m_min.z + j;
					
					GL.Vertex(v0);
					GL.Vertex(v1);
				}
			}
		} else if (axis == tools.Axis.Y) {
			v0.y = m_min.y;
			v1.y = m_max.y;
		  	for (int i = 0; i < size.x; i += 5){
				v0.x = v1.x = m_min.x + i;
				for (int j = 0; j < size.z; j += 5){
					v0.z = v1.z = m_min.z + j;
					
					GL.Vertex(v0);
					GL.Vertex(v1);
				}
			}
			
		} else if (axis == tools.Axis.Z) {
			v0.z = m_min.z;
			v1.z = m_max.z;
		  	for (int i = 0; i < size.x; i += 5){
				v0.x = v1.x = m_min.x + i;
				for (int j = 0; j < size.y; j += 5){
					v0.y = v1.y = m_min.y + j;
					
					GL.Vertex(v0);
					GL.Vertex(v1);
				}
			}
			
		}
		
		GL.End();
		
	}
	
}

