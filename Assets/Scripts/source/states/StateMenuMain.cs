using UnityEngine;
using System.Collections.Generic;

using ui;

/**
 *	Application state featuring the main menu.
 **/
public class StateMenuMain : State
{
	protected Gui3DStateMenuMainCS m_gui;
	public SplashCS splash;
	
	protected IEventHandler m_handler;
	
	public static bool s_firstTime;
	
	//
	//
	
	static StateMenuMain()
	{
		s_firstTime = true;
#if UNITY_EDITOR
//		s_firstTime = false;
#endif	
	}
	
	
	public StateMenuMain() : base(EState.MENUMAIN)
	{
	}
	
	public override void Init()
	{
		base.Init();
		
		// retrieve gui object
		GameObject go = Camera.mainCamera.transform.Find("gui3D").Find("gui3DStateMenuMain").gameObject;
		if (go != null){	
			go.SetActive(true);
			m_gui = go.GetComponent<Gui3DStateMenuMainCS>();
			if (m_gui != null){
//				m_gui.State = this;
			}
		}
		
		if (s_firstTime){
			s_firstTime = false;
			m_gui.RunSplash();
		} else {
//			m_gui.Abort(); 
		}
		
		m_handler = new EventHandlerMenuMain(this);
	}
	
	public override void Cleanup()
	{
		m_gui.gameObject.SetActive(false);
		
		base.Cleanup(); 
	}
	
	// Update is called once per frame
	public override void Update (float seconds) 
	{
		base.Update(seconds); 
	}
	
	public void AbortSplash()
	{
		this.m_gui.AbortSplash();
	}
	
	
	public override bool HandleEvent(UnityEngine.Event e)
	{
		bool handled = false;
		if (!handled){
			handled = this.m_gui.HandleEvent(e);	
		}
		if (!handled){
			handled = this.m_handler.HandleEvent(e);	
		}
		return handled;
	}
}
