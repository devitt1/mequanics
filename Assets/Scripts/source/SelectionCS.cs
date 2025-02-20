using UnityEngine;
using System.Collections;

using tools;

public class SelectionCS : MonoBehaviour {
	
	protected VectorInt3 m_pos;
	protected Vector3 m_offset;
	
	// Use this for initialization
	void Start () 
	{
		m_pos = new VectorInt3(transform.position);
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	void OnTriggerEnter(Collider other)
	{
//		UnityEngine.Debug.Log("trigger entered : " + other.gameObject.name);
	}
	
	public void ResetOffset()
	{
		m_offset = Vector3.zero;
		
		UpdatePos();
	}
	
	public void SetOffset(Vector3 v)
	{
		m_offset = v;
		
		UpdatePos();
	}
	
	public void MovePos(Direction d)
	{
		this.m_pos += Tools.GetVectorFromDirection(d);
		this.m_offset -= Tools.GetVectorFromDirection(d).ToVector3();
	
		UpdatePos();
	}
	
	void UpdatePos()
	{
		this.transform.position = this.m_pos.ToVector3() + m_offset;
	}
}
