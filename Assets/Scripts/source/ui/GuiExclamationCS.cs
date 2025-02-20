using UnityEngine;
using System;

using ui;


/**
 * 
 * */
public class GuiExclamationCS : GuiCS
{
	protected string m_message = "";
	public string Message { set { m_message = value; } }
	protected float m_duration = 3;
	public float Duration { set { m_duration = value; } }
	
	protected Rect m_rectText;
	
	protected bool m_done;
	
	//
	//
	
	// Use this for initialization
	public override void Start() 
	{
		base.Start ();
		
		m_done = false;
	}
	
	public void Update () 
	{
		if (m_done){
			GameObject.Destroy(this.gameObject);
		}
	}
	
	void OnGUI () {
		if (!m_done)
		{	
			GUI.Label(m_rectText, m_message);
			
		}
	}
	
}


