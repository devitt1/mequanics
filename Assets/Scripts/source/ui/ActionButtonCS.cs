using UnityEngine;
using System.Collections;

using ui;

public class ActionButtonCS : MonoBehaviour 
{	
	public string m_label;
	public string m_hotkey;
	public KeyCode m_keycode;
	public bool m_enabled;
	public ButtonCB m_callback;
	public int m_index;
	public Collider m_coll;
	
	protected GameObject m_bg;
	
	//
	//
	
	// Use this for initialization
	public void Start () 
	{
		m_bg = transform.GetChild(0).gameObject;
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	public void OnMouseEnter()
	{
		// change label text
		this.transform.parent.parent.GetComponent<Gui3DStatePlayCS>().SetActionString(this.m_label);
		
		SummonBackground();
	}
	
	public void OnMouseExit()
	{
		// change label text
		this.transform.parent.parent.GetComponent<Gui3DStatePlayCS>().SetActionString("");
		
		ReleaseBackground();

	}
	
	protected void SummonBackground()
	{
		// slide background 
		AnimationState state = m_bg.GetComponent<Animation>()["actionButtonSlide"];
		state.enabled = true;
		state.weight = 1f;
		state.normalizedTime = 0f;
		state.speed = 1f;
		state.wrapMode = WrapMode.Once;
	}
	
	protected void ReleaseBackground()
	{
		// slide background 
		AnimationState state = m_bg.GetComponent<Animation>()["actionButtonSlide"];
		state.enabled = true;
		state.weight = 1f;
		state.normalizedTime = 1f;
		state.speed = -1f;
		state.wrapMode = WrapMode.Once;
	}
	
}