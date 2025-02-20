using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using pcs;

/**
 * 
 * */
public abstract class PieceCS : MonoBehaviour 
{
	public Piece piece;
	
	public static Material s_materialWireframe;
	public Material MatWireframe { 
		get{
			if (s_materialWireframe == null) {
				InitWireframeMaterial();
			}
			return s_materialWireframe;
		}
	}
	protected List<Vector3> m_wireframe;
	public void SetWireframe(List<Vector3> wireframe)
	{
		m_wireframe = wireframe;
	}
	
	
	public Color backgroundColor = new Color(0.1f ,0.1f, 0.1f, 1.0f);
	public Color lineColor = new Color(1f ,1f, 1f, 1.0f);

	public bool ZWrite = true;
	public bool AWrite = true;
	public bool blend = true;
	
	protected MeshFilter m_meshFilter;
	protected Material m_material;
	protected Transform m_transformMesh;
	protected Collider m_colliderMesh;
	
	public virtual void SetColorVisual(Color c)
	{
		GetMaterial().SetColor("_ColorBlock", c);
	}
	
	public abstract Material GetMaterial();
	
	public abstract MeshFilter GetMesh();

	public abstract Transform GetTransformMesh();
	
	public abstract Collider GetColliderMesh();
	
	//
	//
	
	// Use this for initialization
	protected virtual void Start () 
	{  
	}
	
	protected virtual void InitWireframeMaterial()
	{
		if (s_materialWireframe == null){
			Shader shader = (Shader)Resources.Load("Shaders/Wireframe", typeof(Shader));
			s_materialWireframe = new Material(shader);		
		    s_materialWireframe.hideFlags = HideFlags.HideAndDontSave;
		}
	}
		
	// Update is called once per frame
	void Update () 
	{
	}
		
	public void OnRenderObject()
	{
		if (Piece.s_WireframesOGL){
			if (m_wireframe == null){
				PieceType type = (piece != null ? piece.GetType() : PieceType.TUBE);
				m_wireframe = CircuitRenderer.GetWireframe(type, this.GetMesh());
			}
			
			float timeFactor = Mathf.Sin(CircuitRenderer.t*2f) / 4f + 0.5f;
			
			if (piece != null)
			{
				Color selectColor = new Color(0.4f, 1f, 0.4f, 1f);
		//		Color injectColor = new Color(1f, .7f, 0f, 1f);
				Color unstableColor = new Color(1f, .4f, .4f, 1f);
				
				selectColor *= timeFactor;
		//		injectColor *= timeFactor;
				unstableColor *= timeFactor;
		
				if (piece.IsSelected()){
					lineColor = selectColor;
	//			} else if (circuit.m_movingInjector &&
		//					piece.GetPathId() == circuit.m_movingInjector.GetPathId()) {
		//				lineColor = injectColor;
				} else if (!CircuitRenderer.AreDisjoint(piece.GetTags(), CircuitRenderer.m_unstable)) {
						lineColor = unstableColor;
				} else {
					lineColor = (piece.GetColor() + new Vector4(0.4f, 0.4f, 0.4f, 0f)) * timeFactor;
				}
			} else {
				lineColor = (new Vector4(0.5f, 0.5f, 0.5f, 1f) + new Vector4(0.9f, 0.9f, 0.9f, 0f)) * timeFactor;
			}
			
		    MatWireframe.SetPass(0);
			
			// Invoke OpenGL directly
			{
			    GL.PushMatrix();
			    GL.MultMatrix(GetTransformMesh().localToWorldMatrix);
			    GL.Begin(GL.LINES);
				
			    GL.Color(lineColor);
			   
			    for (int i = 0; i < m_wireframe.Count / 3; i++)
			    {
			        GL.Vertex(m_wireframe[i * 3]);
			        GL.Vertex(m_wireframe[i * 3 + 1]);
			       
			        GL.Vertex(m_wireframe[i * 3 + 1]);
			        GL.Vertex(m_wireframe[i * 3 + 2]);
			       
			        GL.Vertex(m_wireframe[i * 3 + 2]);
			        GL.Vertex(m_wireframe[i * 3]);
			    }
			         
			    GL.End();
			    GL.PopMatrix();
			}
		}
	}
	
	
}
