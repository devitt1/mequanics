using UnityEngine;
using System.Collections;

public class BoundingBoxCS : MonoBehaviour 
{
	public float m_freq = 0f;
	public float s_alphaMin = 0.0f;
	public float s_alphaMax = 0.5f;
		
	protected Material m_mat;
	protected Material Mat { get{ if (m_mat == null) { m_mat = this.renderer.material; } return m_mat; }}
	
	public VectorInt3 Position { set{ transform.position = value.ToVector3(); } } 
	public VectorInt3 Size { set{ transform.localScale = value.ToVector3(); } } 
	
	float m_alphaOffset = 0;
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_freq != 0){
			float f = Mathf.Sin(Time.time * m_freq * Mathf.PI*2) * 0.5f;
			
			Color c = Mat.GetColor("_TintColor");
			c.a = s_alphaMin + (s_alphaMax-s_alphaMin)*f;
			Mat.SetColor("_TintColor", c);
		}
	}
	
	public void SetExitation(float val) 
	{ 
		val = Mathf.Clamp01(val);
		
		m_freq = Mathf.Pow((val * 1.5f), 3f);  
//		m_alphaOffset = val * 0.25f;
		
	}
	
	
}
