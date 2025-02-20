using UnityEngine;
using System.Collections;

using narr;

/**
 * Class managing the app's top level control flow.
 * Never to be unloaded throughout the app lifetime.
 * 
 * Singleton class
 * */
public class StateMachine : MonoBehaviour 
{
	// variables
	
	protected static StateMachine s_sm;
	public static StateMachine Instance { get{ return s_sm; } }
	
	protected State m_state;
	private EState m_stateRequested;
	
	public EState StateRequested
	{
		get {	return m_stateRequested;	}
		set {	m_stateRequested = value;	}
	}
	
	string m_uriCircuit = null;
	int m_slot = -1;
	
	//
	// override methods
	
	public void OnEnable()
	{	
		// State 
		System.Diagnostics.Debug.Assert(Instance == null);
		StateMachine.s_sm = this;
		
		StateRequested = EState.INVALID;
	}
	
	public void OnDisable()
	{
		StateMachine.s_sm = null;
	}
	
	// Use this for initialization
	public void Start () 
	{
		StateRequested = EState.MENUMAIN;
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if (StateRequested != EState.INVALID){
			var stateReq = StateRequested;
			StateRequested = EState.INVALID;
			StartCoroutine(ToState(stateReq));
		} else {
			if (m_state != null){
				m_state.Update(UnityEngine.Time.deltaTime);
			}
		}
	}
	
	//
	// common methods
	
	public void RequestStateChange(EState state)
	{
		// validate request 
		if (state == EState.INVALID || StateRequested != EState.INVALID){
			return;
		}		
		
		StateRequested = state;
	}
	
	protected IEnumerator ToState(EState state)
	{	
		// leaving current state
		if (m_state != null){
			if (m_state.StateType == EState.MENUCIRCUITSELECT)
			{
				m_uriCircuit = ((StateCircuitSelect)m_state).UriCircuit;
				m_slot = ((StateCircuitSelect)m_state).Slot;
			} else if (m_state.StateType == EState.MENUMAIN) {
				
			} else if (m_state.StateType == EState.PLAY) { 
				
			}
			
			GameObject go;
			go = GameObject.FindGameObjectWithTag("gui2D"); 
			if (go != null){	Destroy(go);	}
			go = GameObject.FindGameObjectWithTag("sceneroot"); 
			if (go != null){	Destroy(go);	}
			
			m_state.Cleanup();
			m_state = null; // 
		}
		
		// entering new state
		// TODO move these initialization code to State classes
		if (state == EState.TERMINATE)
		{
			#if UNITY_WEBPLAYER
//				Application.OpenURL(QConfig.s_urlExitWebApp);
			#endif
			Application.Quit();
		
			StateRequested = EState.QUIT;
			
		} 
		else if (state == EState.QUIT)
		{
			Application.Quit();
		} 
		else if (state == EState.MENUMAIN)
		{
			StateRequested = EState.INVALID;
			Application.LoadLevel("menuMain");	
			
			while (Application.isLoadingLevel){
				yield return new WaitForSeconds(0.1f);
			}
	
			m_state = new StateMenuMain();
			m_state.Init();
		} 
		else if (state == EState.MENUCIRCUITSELECT)
		{
			StateRequested = EState.INVALID;
			Application.LoadLevel("menuCircuitSelect");
			
			while (Application.isLoadingLevel){
				yield return new WaitForSeconds(0.1f);
			}
			
			m_state = new StateCircuitSelect();
			m_state.Init();
		} 
		else if (state == EState.PLAY)
		{
			StateRequested = EState.INVALID;
			
			Application.LoadLevel("play");	
			while (Application.isLoadingLevel){
				yield return new WaitForSeconds(0.1f);
			}
			
			m_state = new StatePlay();
			m_state.Init();
			
			if (m_uriCircuit != null){
				Content c = new Content(m_uriCircuit, null);
				((StatePlay)m_state).RequestContent(c);
			} else if (m_slot != -1) {
				((StatePlay)m_state).RequestProgress(m_slot);
			}
		}
		else if (state == EState.PLAYTUTORIAL)
		{
			StateRequested = EState.INVALID;
			
			Application.LoadLevel("play");	
			while (Application.isLoadingLevel){
				yield return new WaitForSeconds(0.1f);
			}
			
			m_state = new StatePlay();
			m_state.Init();
			
			Narrative narrative = new Tutorial(((StatePlay)m_state).GetInterface(), ((StatePlay)m_state).GetInterface());
		
			Content c = new Content(narrative.UriCircuit, narrative);
			((StatePlay)m_state).RequestContent(c);
		} 
		else 
		{
			Debug.LogWarning("WARNING: EState " + state.ToString() + " invalid");
			System.Diagnostics.Debug.Assert(false);		
		}	
		yield break;
	}

}
