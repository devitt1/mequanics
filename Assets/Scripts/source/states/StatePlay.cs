using UnityEngine;

using ui;
using narr;

public struct Content
{
	public string uriCircuit;
	public Narrative narrative;
	
	public Content(string uri, Narrative n) { uriCircuit = uri; narrative = n; }
}

/**
 *	Application state where the player manipulates a circuit 
 *	(in order to optimize it).
 **/
public class StatePlay : State
{
	protected GameInterface m_interface;
	
	protected string m_nameCircuit;
	public string NameCircuit{ get { return m_nameCircuit; } }
	
	//
	//
		
	public StatePlay() : base(EState.PLAY)
	{
		m_interface = new GameInterface(this);
		
		m_nameCircuit = "";
	}
	
	protected Gui3DStatePlayCS m_gui;
	public Gui3DStatePlayCS Gui { 
		get{ 
			if (m_gui == null){
				// retrieve gui object
//				GameObject go = GameObject.FindGameObjectWithTag("gui2D");
				GameObject go = Camera.mainCamera.transform.Find("gui3D").Find("gui3DStatePlay").gameObject;
				if (go != null){
					go.SetActive(true);
					m_gui = go.GetComponent<Gui3DStatePlayCS>();
					if (m_gui != null){
	//					m_gui = ((Gui3DStatePlayCS)go.GetComponent<GuiStatePlayCS>());
						m_gui.State = this;
					}
				}
			}
			return m_gui;
		} 
	}
	
	public GameInterface GetInterface()
	{
		return m_interface;	
	}
	//
	//
	
	public override void Init()
	{
		base.Init();
	}
	
	public override void Cleanup()
	{
		m_gui.Cleanup();
		m_gui.gameObject.SetActive(false);
		
		m_interface.Cleanup();
		
		base.Cleanup(); 
	}
	
	// Update is called once per frame
	public override void Update (float seconds) 
	{
		base.Update(seconds);
		
		m_interface.Update(seconds);
	}
	
	//
	//
	
	public void RequestProgress(int slot)
	{
		m_interface.RequestProgress(slot);
	}
	
	public void RequestContent(Content c)
	{
		m_interface.RequestContent(c);
	}
	
	public void NotifyUriCircuit(string uri)
	{
		this.m_nameCircuit = System.IO.Path.GetFileName(uri);
		this.m_gui.Score1 = this.m_interface.GetScore1();
		this.m_gui.CircuitName = System.IO.Path.GetFileNameWithoutExtension(m_nameCircuit);
	}
	
	public void SetSoftKeyDown(KeyCode key, bool down)
	{
		m_interface.SetSoftKeyDown(key, down);
	}
	
	public override bool HandleEvent(UnityEngine.Event e)
	{
		bool handled = false;
		if (!handled){
			handled = this.m_gui.HandleEvent(e);	
		}
		if (!handled){
			handled = this.m_interface.m_interpreter.m_eventHandler.HandleEvent(e);	
		}
		return handled;
	}
}
