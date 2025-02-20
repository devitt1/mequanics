using UnityEngine;
using System.Collections.Generic;

using ui;

public class ActionButtonToggleCS : ActionButtonCS
{
	public ButtonCB m_callbackReleased;
	
	protected bool m_activated;
	protected Gui3DStatePlayCS m_gui;
	
	// Use this for initialization
	public void Start () 
	{
		base.Start();
		
		m_gui = this.transform.parent.parent.GetComponent<Gui3DStatePlayCS>();
		m_activated = false;
	}
	
	public void OnMouseExit()
	{
		if (!m_activated){
			base.OnMouseExit();
		}
	}
	
	public void Activate()
	{
		m_activated = true;
	}
	
	public void Deactivate()
	{
		m_callbackReleased(this.m_keycode);
			
		// change label text
		if (m_activated){
			if (m_gui.ActionString == m_label){
				m_gui.SetActionString("");
			}
			
			ReleaseBackground();
		}
	}
}

