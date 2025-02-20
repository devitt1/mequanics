using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ui;


/**
 * 
 * */
public class Gui3DStateMenuCircuitCS : GuiCS, IEventHandler
{
	public class ButtonInfo 
	{
		public string 		m_name;
		public ButtonCB 	m_callback;
		public bool			m_user;
		public bool 		m_local;
		public Collider		m_coll;
	}
	
	//
	//
	
	protected LinkedList<ButtonInfo> m_buttons = new LinkedList<ButtonInfo>();
	
	public Collider[] m_bnCircuits = new Collider[20];
	public Collider m_bnBack;
	public Collider m_bnServerUrl;
	
	protected string m_url = QConfig.s_urlUserDefault;
	protected bool m_urlUpdating = false;
	
	protected static int s_posBoxUrlX; 
	
	//
	//
	
	public override void Start()
	{	
		m_urlUpdating = false;
	}
	
	public void Update()
	{
		m_state.ShowBusy(m_urlUpdating);
	}	
	
	//
	//
	
	public bool HandleEvent(UnityEngine.Event e)
	{		
		switch (e.type){
		case EventType.MouseDown:
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("gui3D"))){
				if (hit.collider == this.m_bnBack){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
					StateMachine.Instance.RequestStateChange(EState.MENUMAIN);
				} else {
					foreach (ButtonInfo bi in m_buttons){
						if (hit.collider == bi.m_coll)
						{
							SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
							bi.m_callback(bi.m_name);
							break;
						}
					}
				}
				return true;
			}
			break;
		default:
			break;
		}
		return false;
	}
	
		
	void OnGUI () 
	{
		// URL box
		GUI.enabled = true;
		Vector2 scale = new Vector2(Screen.width/960f, Screen.height/600f);
		Rect rectServer = new Rect(scale.x*230f, scale.y*62f, scale.x*640f, scale.y*30f);
		m_url = GUI.TextField(rectServer, m_url);
		if(Event.current.type == EventType.layout && Event.current.keyCode == KeyCode.Return) {		
			UpdateCircuitsWeb();
		}
	}
	
	public void UpdateCircuitsWeb()
	{
		if (!m_urlUpdating)
		{
			
			// remove buttons refering to server-based circuits 
			UnregisterCircuitButtonsWeb();
			
			if (m_url == ""){
				return;
			}
			
			SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSwoosh);
			
			m_urlUpdating = true;
			
			// get directory listing from new location
			if (!m_url.StartsWith("http://") && !m_url.StartsWith("https://")){
				m_url = "http://" + m_url;
			}
			if (!m_url.EndsWith("/")){
				m_url = m_url + "/";
			}
			StartCoroutine(GetServerDirContents());
		}
	}
	
	protected IEnumerator GetServerDirContents()
	{	
		var www = new WWW(m_url);
		yield return www;
		
		if (www.error != null){
			Popups.GenerateError("Trying to retrieve url directory contents yielded the Error:\n'" + www.error + "'");
			SoundHubCS.Instance.PlaySE(GuiCS.SoundClickFailed);
			m_urlUpdating = false;
		} else {
			HashSet<string> cirFilesOnServer = new HashSet<string>();
			
			// Identify circuit files
			MatchCollection matchesCir = Regex.Matches(www.text, @"\W\w+\.cir\W", RegexOptions.IgnoreCase);
			foreach(Match match in matchesCir){
				foreach ( System.Text.RegularExpressions.Group gr in match.Groups){
					string circuit = gr.ToString();
					circuit = circuit.Substring(0, circuit.Length - 1).Substring(1);
					cirFilesOnServer.Add(m_url + circuit);
				}
			}
			MatchCollection matchesTex = Regex.Matches(www.text, @"\W\w+\.tex\W", RegexOptions.IgnoreCase);
			foreach(Match match in matchesTex){
				foreach ( System.Text.RegularExpressions.Group gr in match.Groups){
					string circuit = gr.ToString();
					circuit = circuit.Substring(0, circuit.Length - 1).Substring(1);
					cirFilesOnServer.Add(m_url + circuit);
				}
			}
			
			// Register new buttons
			UpdateCircuitButtonsWeb(cirFilesOnServer);
			
			m_urlUpdating = false;
		}
	}
	
	protected void UpdateCircuitButtonsWeb(HashSet<string> circuits)
	{
		// diplay new buttons
		foreach (string s in circuits){
			RegisterCircuitButton(s, ((StateCircuitSelect)m_state).HandleCircuitSelect, true, false);
		}
		
	}
	
	//
	//
	
	public delegate void ButtonCB(string nameCircuit);
	
	public bool RegisterCircuitButton(string name, ButtonCB callback, bool user, bool local)
	{
		int i = m_buttons.Count;
		if (i < 20){			
			ButtonInfo bi = new ButtonInfo{m_name = name, m_callback = callback, m_user = user, m_local = local};
			
			// activate button
			bi.m_coll = this.m_bnCircuits[i];
			bi.m_coll.transform.GetChild(0).GetComponent<TextMesh>().text = System.IO.Path.GetFileNameWithoutExtension(bi.m_name);
			bi.m_coll.gameObject.SetActive(true);
			
			m_buttons.AddLast(bi);
			return true;
		} else {
			return false;
		}
	}
	
	public void UnregisterCircuitButtonsWeb()
	{
		var node = m_buttons.First;
		while(node != null){
			var nextNode = node.Next;
			if (node.Value.m_user && !node.Value.m_local){
				node.Value.m_coll.gameObject.SetActive(false);
				m_buttons.Remove(node);
			}
			node = nextNode;
		}
	}
	
	public void UnregisterCircuitButtonsAll()
	{
		var node = m_buttons.First;
		while(node != null){
			var nextNode = node.Next;

			node.Value.m_coll.gameObject.SetActive(false);
			m_buttons.Remove(node);
		
			node = nextNode;
		}
	}
	
	
}