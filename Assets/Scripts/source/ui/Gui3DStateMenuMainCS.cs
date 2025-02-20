using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ui;

/**
 * 
 * */
public class Gui3DStateMenuMainCS : GuiCS, IEventHandler
{
	public Collider m_bnStart;
	public Collider m_bnQuit;
	public Collider m_bnTutorial;
	public Collider m_bnVersion;
	
	public SplashCS m_splash;
	
	//
	//
	
	public override void Start() 
	{
		m_bnVersion.transform.GetChild(0).GetComponent<TextMesh>().text = QConfig.s_versionComplete;
	}
	
	protected void SetEnable(bool enable)
	{
		m_bnStart.enabled = enable;
		m_bnQuit.enabled = enable;
		m_bnTutorial.enabled = enable;
		m_bnVersion.enabled = enable;
	}
	
	public void RunSplash()
	{
		if (m_splash != null){
//			SetEnable(false);
		
			m_splash.Run();
		}
	}
	
	public void AbortSplash()
	{
//		SetEnable(true);

		if (m_splash != null){
			m_splash.Abort();
		}
		
	}
	
	void Update() 
	{	
//		if (!m_bnStart.enabled){
//			if (m_splash != null && m_splash.gameObject.activeSelf){
//				SetEnable(true);
//			}
//		}
	}
	
	//
	//
	
	/// <summary>
	/// Check for mouseclick hitting any of the buttons on the main menu
	/// </param>
	public bool HandleEvent(UnityEngine.Event e)
	{		
		switch (e.type){
		case EventType.MouseDown:
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("gui3D"))){
				if (hit.collider == this.m_bnStart){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
					StateMachine.Instance.RequestStateChange(EState.MENUCIRCUITSELECT);
				} else if (hit.collider == this.m_bnTutorial){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
					StateMachine.Instance.RequestStateChange(EState.PLAYTUTORIAL);
				} else if (hit.collider == this.m_bnQuit){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickFailed);
//					StateMachine.Instance.RequestStateChange(EState.TERMINATE);
				} else if (hit.collider == this.m_bnVersion){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickFailed);
					//TODO show something here
					return false; 
				} 
				return true;
			}
			break;
		default:
			break;
		}
		return false;
	}
	
}