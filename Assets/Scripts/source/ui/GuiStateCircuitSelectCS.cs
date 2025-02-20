using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ui;


/**
 * 
 * */
public class GuiStateCircuitSelectCS : GuiCS
{
	public class ButtonInfo 
	{
		public string 		m_name;
		public ButtonCB 	m_callback;
		public bool			m_user;
		public bool 		m_local;
	}
	
	//
	//
	
	protected LinkedList<ButtonInfo> m_buttons;
	
	protected static int sizeUrlX = 320;
	protected static int posBoxBackX = 10;
	protected static int posBoxBackY = 10;
	protected static int posBoxCircuitsX = 10;
	protected static int posBoxCircuitsY = 60;
	protected static int posBoxUrlX; 
	protected static int posBoxUrlY = 10;
	
	protected string m_url;
	protected bool m_urlUpdating;
	
	//
	//
	
	static GuiStateCircuitSelectCS()
	{
		// scale GUI controls
		sizeUrlX = (int)(Scale * sizeUrlX);
		posBoxBackX = (int)(Scale * posBoxBackX);
		posBoxBackY = (int)(Scale * posBoxBackY);
		posBoxCircuitsX = (int)(Scale * posBoxCircuitsX);
		posBoxCircuitsY = (int)(Scale * posBoxCircuitsY);
		posBoxUrlX = s_sizeButtonX + 4 * s_sizeSpacing;
		posBoxUrlY = (int)(Scale * posBoxUrlY);
	}
	
	//
	//
	
	public override void Start()
	{	
		m_buttons = new LinkedList<ButtonInfo>();
		m_url = QConfig.s_urlUserDefault;
		m_urlUpdating = false;
	}
		
	void OnGUI () 
	{
		// Set font for all styles
//		GUI.skin.font = (Font)Resources.Load("Fonts/arial", typeof(Font));
		GUI.skin = this.Skin;
		
		// Back button
		GUI.enabled = true;
		GUI.Box(new Rect(s_sizeSpacing,s_sizeSpacing,s_sizeButtonX + 2*s_sizeSpacing, s_sizeButtonY + 2*s_sizeSpacing), "");
		if(ButtonWithSound(new Rect(2*s_sizeSpacing, 2*s_sizeSpacing, s_sizeButtonX, s_sizeButtonY), Locale.Str(PID.BN_BACK))) {
			StateMachine.Instance.RequestStateChange(EState.MENUMAIN);
		}	
		
		// Circuit selection box
		//
		// Background
		if (m_buttons != null)
		{
			int nCircuits = m_buttons.Count;
			int rows = (Screen.height - (s_sizeButtonY + 6*s_sizeSpacing) - (s_sizeSpacing+s_sizeTextY)) / (s_sizeSpacing + s_sizeButtonY);
			int cols = (int)Mathf.Ceil(((float)(nCircuits)) / rows);
			int sizeBoxX = cols * (s_sizeButtonX + s_sizeSpacing) + (s_sizeSpacing);
			int sizeBoxY = s_sizeSpacing + (s_sizeSpacing + s_sizeTextY) + (rows) * (s_sizeButtonY + s_sizeSpacing);
			GUI.Box(new Rect(posBoxCircuitsX, posBoxCircuitsY, sizeBoxX, sizeBoxY), "\n" + Locale.Str(PID.SelectACircuit));
			
			// Prepare different GUI styles for user defined and bundled circuits
			
			// bundled circuits
			GUIStyle styleBundled = new GUIStyle(GUI.skin.button);
			styleBundled.fontStyle = FontStyle.Italic;
			Color c = GUI.skin.button.normal.textColor;
			styleBundled.normal.textColor = 
				styleBundled.hover.textColor = 
				styleBundled.active.textColor = new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, c.a * 1.0f);
			// local storage custom circuits
			GUIStyle styleCustomLocal = new GUIStyle(GUI.skin.button);
			// custom circuits on the web
			GUIStyle styleCustomWeb = new GUIStyle(GUI.skin.button);
			styleCustomWeb.fontStyle = FontStyle.Bold;
//			GUIStyle styleBundled = this.Skin.customStyles[0];
//			GUIStyle styleCustomLocal = this.Skin.customStyles[1];
//			GUIStyle styleCustomWeb = this.Skin.customStyles[2];
			//
			// Circuit buttons
			int line = 0; 
			foreach (ButtonInfo bi in m_buttons){
				string filename = bi.m_name;
				VectorInt2 posGrid = new VectorInt2(line / rows, line % rows);
				Rect rect = new Rect(posBoxCircuitsX + s_sizeSpacing + posGrid.x*(s_sizeSpacing+s_sizeButtonX), 
					posBoxCircuitsY + s_sizeTextY + 2*s_sizeSpacing + posGrid.y * (s_sizeButtonY + s_sizeSpacing), 
					s_sizeButtonX, 
					s_sizeButtonY);
				if (ButtonWithSound(rect, System.IO.Path.GetFileName(filename), (bi.m_user ? (bi.m_local ? styleCustomLocal : styleCustomWeb) : styleBundled))) {
					if (m_state != null){
						bi.m_callback(bi.m_name);
					}
				}
				line++;
			}
		}
		
		// URL box
		GUI.enabled = true;
		GUI.Box(new Rect(posBoxUrlX,posBoxUrlY, sizeUrlX + 2 * s_sizeSpacing, s_sizeTextY + 2 * s_sizeSpacing), "");
		m_url = GUI.TextField(new Rect(posBoxUrlX + s_sizeSpacing, posBoxUrlY + s_sizeSpacing, sizeUrlX, s_sizeTextY), m_url);
		if(Event.current.type == EventType.layout && Event.current.keyCode == KeyCode.Return) {
			UpdateCircuitsWeb();
		}
	}
	
	public void UpdateCircuitsWeb()
	{
		if (!m_urlUpdating)
		{
			m_urlUpdating = true;
			m_state.ShowBusy(m_urlUpdating);
			
			// remove buttons refering to server-based circuits 
			UnregisterCircuitButtonsWeb();
			
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
//			Popups.GenerateError("Trying to retrieve url directory contents yielded the Error:\n'" + www.error + "'", 3.0f);
			Popups.GenerateError("Trying to retrieve url directory contents yielded the Error:\n'" + www.error + "'");
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
		}
	}
	
	protected void UpdateCircuitButtonsWeb(HashSet<string> circuits)
	{
		// diplay new buttons
		foreach (string s in circuits){
			RegisterCircuitButton(s, ((StateCircuitSelect)m_state).HandleCircuitSelect, true, false);
		}
		
		m_urlUpdating = false;
		m_state.ShowBusy(m_urlUpdating);
	}
	
	//
	//
	
	public delegate void ButtonCB(string nameCircuit);
	
	public void RegisterCircuitButton(string name, ButtonCB callback, bool user, bool local)
	{
		ButtonInfo bi = new ButtonInfo{m_name = name, m_callback = callback, m_user = user, m_local = local};
		m_buttons.AddLast(bi);
	}
	
	public void UnregisterCircuitButtonsWeb()
	{
		var node = m_buttons.First;
		while(node != null){
			var nextNode = node.Next;
			if (node.Value.m_user && !node.Value.m_local){
				m_buttons.Remove(node);
			}
			node = nextNode;
		}
	}
	
	
}