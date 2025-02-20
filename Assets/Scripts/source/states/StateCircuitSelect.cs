using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using ui;

/**
 *	Application state featuring the main menu.
 **/
public class StateCircuitSelect : State, IEventHandler
{
	enum OWNER 
	{
		USER,
		GAME,
	}
	
	//
	// variables
	
	public string UriCircuit { get; set; }
	public int Slot { get; set; }
	
	protected Gui3DStateMenuCircuitCS m_gui;
	
	protected List<string> m_circuitsBundled; // circuits bundled with the app
	protected List<string> m_circuitsCustom; // circuits provided by the user (online or locally)
	protected List<string> m_circuitsCustomLocal; // circuits provided by the user locally 
	
	protected IEventHandler m_handler;
	
	//
	//
	
	public StateCircuitSelect() : base(EState.MENUCIRCUITSELECT)
	{		
	}
	
	public override void Init()
	{
		base.Init();
		
		UriCircuit = null;
		Slot = -1;
		
		// retrieve gui reference
		GameObject go = Camera.mainCamera.transform.Find("gui3D").Find("gui3DStateMenuCircuit").gameObject;
		if (go != null){
			go.SetActive(true);
			m_gui = go.GetComponent<Gui3DStateMenuCircuitCS>();
			if (m_gui != null){
				m_gui.State = this;
			}
		} 
		
		m_handler = new EventHandlerMenuCircuit(this);
		
		// Gather information about available circuits
		
		m_circuitsCustom = new List<string>();
		m_circuitsCustomLocal = new List<string>();
		
		#if ! UNITY_WEBPLAYER
		// custom files on local disk
		// gather
		m_circuitsCustom.AddRange(System.IO.Directory.GetFiles(Application.persistentDataPath, "*.cir"));
		m_circuitsCustom.AddRange(System.IO.Directory.GetFiles(Application.persistentDataPath, "*.tex"));
		foreach (int i in Enumerable.Range(0, m_circuitsCustom.Count)){
			m_circuitsCustom[i] = m_circuitsCustom[i].Replace(@"\", "/");
			m_circuitsCustom[i] = System.IO.Path.GetFileName(m_circuitsCustom[i]);
			m_circuitsCustomLocal.Add(m_circuitsCustom[i]);
		}
		#endif		
		
		// circuits bundled with client
		// gather files
		m_circuitsBundled = new List<string>();
		var circuitsResource = Resources.LoadAll("Circuits", typeof(UnityEngine.TextAsset));
		for (int i=0; i < circuitsResource.Length; ++i){	
			string uri = "Circuits/" + ((TextAsset)circuitsResource[i]).name;
			m_circuitsBundled.Add(uri);
		}
		
		// register buttons 
		
		// bundled circuits
		m_circuitsBundled.Sort((s1, s2) => s2.CompareTo(s1));
		foreach (string name in m_circuitsBundled){
			m_gui.RegisterCircuitButton(name, this.HandleCircuitSelect, false, true);
		}
		
		// custom local circuits 
		m_circuitsCustom.Sort((s1, s2) => s2.CompareTo(s1));
		foreach (string name in m_circuitsCustom){
			m_gui.RegisterCircuitButton(name, this.HandleCircuitSelect, true, true);
		}
		
		// custom circuits on a web-server
		// gather and register (async)
		m_gui.UpdateCircuitsWeb();
	}
	
	public override  void Cleanup()
	{
		m_gui.UnregisterCircuitButtonsAll();
		
		m_gui.gameObject.SetActive(false);
		
		base.Cleanup(); 
	}
	
	public void HandleCircuitSelect(string name)
	{
		if (m_circuitsCustomLocal.Contains(name)){
			UriCircuit = Application.persistentDataPath + "/" + name;
		} else {
			UriCircuit = name;
		}
		StateMachine.Instance.RequestStateChange(EState.PLAY);
	}
	
	public void HandleSavegameSelect(int slot)
	{
		Slot = slot;
		StateMachine.Instance.RequestStateChange(EState.PLAY);
	}
	
	public override void Update (float seconds) 
	{
		base.Update(seconds); 
	}
	
	public override bool HandleEvent(UnityEngine.Event e)
	{
		bool handled = false;
		if (!handled) {
			handled = m_gui.HandleEvent(e);
		}
		if (!handled) {
			handled = m_handler.HandleEvent(e);
		}
		return handled;
	}
}
