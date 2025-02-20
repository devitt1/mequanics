
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ui;

// callback function prototype
public delegate void ButtonCB(KeyCode keycode);

/**
 * 
 * */
public class GuiStatePlayCS : GuiCS
{
	protected class ButtonInfo
	{
		public string m_label;
		public string m_hotkey;
		public KeyCode m_keycode;
		public bool m_enabled;
		public Rect m_rect;
		public HudPos m_hudPos;
		public ButtonCB m_callback;
		public int m_index;
	}
	
	public new StatePlay State{
		set{ m_state = value; }
		get{ return (StatePlay) this.m_state; }
	}
	
	//
	// variables
	
	protected Dictionary<string, ButtonInfo> m_actionButtons = new Dictionary<string, ButtonInfo>();
	
	protected int m_score1 = 0;
	public int Score1 { set { m_score1 = value; } }
	
	protected int m_score2 = 0;
	public int Score2 { set { m_score2 = value; } }
	
	protected bool m_injOk = true;
	public bool InjectionOk { set { m_injOk = value; } }
	
	protected bool m_stable = true;
	public bool Stable { set { m_stable = value; } }
	
	bool m_dragging;
	Rect m_rectDrag;
	Vector3 m_posDragSelectBegin;
	Vector3 m_posDragSelectEnd;
	Material m_matDragBox;
	
	protected static int posBoxBackX = 10;
	protected static int posBoxBackY = 10;
	
	protected static int sizeStatsX = 90;
	protected static int posBoxStatsX = 10;
	protected static int posBoxStatsY = 60;
	
	protected static int sizeCtrlX = 80;
	protected static int sizeCtrlY = 60;
	
	protected bool m_ctrlDown;
	protected bool m_altDown;
	protected bool m_shiftDown;
	
	GUIStyle m_styleToggleActive;
	GUIStyle m_styleToggleInactive;
	
	protected VectorInt2 m_sizeScreen;
	
	//
	// override methods
	
	static GuiStatePlayCS()
	{
		//GuiCS();
		
		// scale GUI controls
		posBoxBackX = (int)(Scale * posBoxBackX);
		posBoxBackY = (int)(Scale * posBoxBackY);
		
		sizeStatsX = (int)(Scale * sizeStatsX);
		posBoxStatsX = (int)(Scale * posBoxStatsX);
		posBoxStatsY = (int)(Scale * posBoxStatsY);
		
		sizeCtrlX = (int)(Scale * sizeCtrlX);
		sizeCtrlY = (int)(Scale * sizeCtrlY);
	}
	
	//
	//
	
	public void OnEnable()
	{	
		Shader shader = Shader.Find("Self-Illumin/VertexLit");
		m_matDragBox = new Material(shader);
		m_matDragBox.color = Color.white;
	
		m_rectDrag = new Rect();
		m_dragging = false;
		
		m_ctrlDown = false;
		m_altDown = false;
		m_shiftDown = false;
		
		m_sizeScreen = new VectorInt2(Screen.width, Screen.height);
	}
	
	public override void Start()
	{
	}
	
	public void OnGUI () 
	{	
		GUI.skin = this.Skin;
//		GUI.skin.font = (Font)Resources.Load("Fonts/arial", typeof(Font));
		
		// Back button
		//
		Rect rcBoxBack = new Rect(posBoxBackX, posBoxBackX, s_sizeButtonX + 2 * s_sizeSpacing, s_sizeButtonY + 2 * s_sizeSpacing);
		GUI.Box(rcBoxBack, "");
		Rect rcButtonBack = new Rect(posBoxBackX + s_sizeSpacing, posBoxBackY + s_sizeSpacing, s_sizeButtonX, s_sizeButtonY);
		if(ButtonWithSound(rcButtonBack, Locale.Str(PID.BN_BACK))) {
			StateMachine.Instance.RequestStateChange(EState.MENUCIRCUITSELECT);
		}	
		
		// Box to contain stats
		//
		Rect rcBoxStats = new Rect(posBoxStatsX, posBoxStatsY, sizeStatsX + 2*s_sizeSpacing, s_sizeTextY + 2 * s_sizeSpacing);
		GUI.Box(rcBoxStats, "");
		// Current core
		string textHud = Locale.Str(PID.Score) + " : " + m_score1 + " | " + m_score2;
		textHud += ( this.m_injOk ? " INJ" : "");
		textHud += ( this.m_stable ? " STA" : "");
		Rect rcTextStats = rcBoxStats;
		rcTextStats.width -= 2 * s_sizeSpacing;
		rcTextStats.height -= 2 * s_sizeSpacing;
		rcTextStats.x += s_sizeSpacing;
		rcTextStats.y += s_sizeSpacing;
		GUI.Label(rcTextStats, textHud);
		
		// Action buttons. 
		//
		if (m_sizeScreen.x != Screen.width || m_sizeScreen.y != Screen.height)
		{
			RecalcButtonPlacement();
		}
		foreach (var b in m_actionButtons){
			ButtonInfo bi = b.Value;
			GUI.enabled = bi.m_enabled;
			
#if UNITY_IPHONE || UNITY_ANDROID
			if(ButtonWithSound(bi.m_rect, bi.m_label)) {	
#else
			if(ButtonWithSound(bi.m_rect, bi.m_label + " (" + bi.m_hotkey + ")")) {	
#endif
				bi.m_callback(bi.m_keycode);
			}
		}
		
#if UNITY_IPHONE || UNITY_ANDROID
		// Soft buttons for CTRL ALT SHIFT
		//
		if (State != null)
		{
			m_styleToggleActive = new GUIStyle(GUI.skin.button);
			m_styleToggleActive.fontStyle = FontStyle.Bold;
			m_styleToggleInactive = new GUIStyle(GUI.skin.button);
			
			// SHIFT
			Rect rcShiftL = new Rect(-5, Screen.height + 5 - s_sizeSpacing - 2*sizeCtrlY, sizeCtrlX,	sizeCtrlY);
			bool shiftDown = GUI.Toggle(rcShiftL, m_shiftDown, "Shift", m_shiftDown ? m_styleToggleActive : m_styleToggleInactive);
			if (shiftDown != m_shiftDown){
				m_shiftDown = shiftDown;
				((StatePlay)m_state).SetSoftKeyDown(KeyCode.LeftShift, m_shiftDown);
			}
			// CTRL
			Rect rcCtrlL = new Rect(-5, Screen.height + 5 - sizeCtrlY, sizeCtrlX,	sizeCtrlY);
			bool ctrlDown = GUI.Toggle(rcCtrlL, m_ctrlDown, "Ctrl", m_ctrlDown ? m_styleToggleActive : m_styleToggleInactive);
			if (ctrlDown != m_ctrlDown){
				m_ctrlDown = ctrlDown;
				((StatePlay)m_state).SetSoftKeyDown(KeyCode.LeftControl, m_ctrlDown);
			}
			// ALT
			Rect rcAltL = new Rect(-5 + sizeCtrlX + s_sizeSpacing, Screen.height + 5 - sizeCtrlY, sizeCtrlX, sizeCtrlY);
			bool altDown = GUI.Toggle(rcAltL, m_altDown, "Alt", m_altDown ? m_styleToggleActive : m_styleToggleInactive);
			if (altDown != m_altDown){
				m_altDown = altDown;
				((StatePlay)m_state).SetSoftKeyDown(KeyCode.LeftAlt, m_altDown);
			}
		}
#endif
			
		// circuit name
		if (State != null){
			GUI.Label(new Rect(Screen.width/2 - 40, 20, 400, 40), ((StatePlay)m_state).NameCircuit);
		}
	}
		

	public void OnRenderObject()
	{
		if (m_dragging) 
		{	
			// Draw selection box
			m_posDragSelectEnd = Input.mousePosition;
			
			Vector2 other1 = new Vector2(m_posDragSelectBegin.x, m_posDragSelectEnd.y); 
			Vector2 other2 = new Vector2(m_posDragSelectEnd.x, m_posDragSelectBegin.y); 
			
			m_matDragBox.SetPass(0);
			
			// Invoke OpenGL directly
			{
			    GL.PushMatrix();
				GL.LoadPixelMatrix();
			    GL.Begin(GL.LINES);
			    {
			        GL.Vertex(m_posDragSelectBegin);
					GL.Vertex(other1);
					GL.Vertex(other1);
			        GL.Vertex(m_posDragSelectEnd);
			        GL.Vertex(m_posDragSelectEnd);
					GL.Vertex(other2);
					GL.Vertex(other2);
			        GL.Vertex(m_posDragSelectBegin);
			    }   
			    GL.End();
			    GL.PopMatrix();
			}
		}
		
	}
		
	//
	// common methods
	
	public void RegisterActionButton(string label, ButtonCB callback, KeyCode keycode, bool enabled)
	{
		RegisterActionButton(label, "", callback, keycode, enabled, HudPos.TOPRIGHT);
	}
		
	public void RegisterActionButton(string label, string hotkey, ButtonCB callback, KeyCode keycode, bool enabled)
	{
		RegisterActionButton(label, hotkey, callback, keycode, enabled, HudPos.TOPRIGHT);
	}
		
	public void RegisterActionButton(string label, string hotkey, ButtonCB callback, KeyCode keycode, bool enabled, HudPos hudPos)
	{
		string strHotKey = hotkey != "" ? hotkey : keycode.ToString();
		ButtonInfo bi = new ButtonInfo{m_label = label, m_hotkey = strHotKey, m_keycode = keycode, m_enabled = enabled, m_callback = callback, m_hudPos = hudPos, m_index = m_actionButtons.Count};
		CalcButtonRect(ref bi);
		m_actionButtons[bi.m_label] =  bi;
	}
	
	protected void CalcButtonRect(ref ButtonInfo bi)
	{
		HudPos hudPos = bi.m_hudPos;
		if (hudPos != HudPos.BOTTOMRIGHT){
			bi.m_rect = new Rect(Screen.width-s_sizeButtonX-s_sizeSpacing, s_sizeSpacing + (s_sizeButtonY+s_sizeSpacing) * bi.m_index, s_sizeButtonX, s_sizeButtonY);
		} else { // default to HudPos.TOPRIGHT
			bi.m_rect = new Rect(Screen.width-s_sizeButtonX-s_sizeSpacing, Screen.height - (s_sizeSpacing + s_sizeButtonY), s_sizeButtonX, s_sizeButtonY);
		}
	}
		
	protected void RecalcButtonPlacement()
	{	
		foreach (var k in m_actionButtons.Keys)
		{
			ButtonInfo bi = m_actionButtons[k];
			CalcButtonRect(ref bi);
		}
	}
		
	public void EnableButton(string label, bool enabled)
	{
		if (m_actionButtons.ContainsKey(label)) {
			m_actionButtons[label].m_enabled = enabled;
		}
	}
	
	public void BeginDragSelect()
	{
		m_posDragSelectBegin = Input.mousePosition;
		m_rectDrag.Set(m_posDragSelectBegin.x, m_posDragSelectBegin.y, 0, 0);
		m_dragging = true;
	}
	
	public Rect EndDragSelect()
	{
		m_dragging = false;
		
		m_rectDrag.width = m_posDragSelectEnd.x - m_posDragSelectBegin.x;
		m_rectDrag.height = m_posDragSelectEnd.y - m_posDragSelectBegin.y;
		return m_rectDrag;
	}
		
	//
	// imported from Hud
		
	public void SetScores(int score1, int score2, bool inj, bool stable)
	{	
		Score1 = score1;
		Score2 = score2;
		Stable = stable;
		InjectionOk = inj;
	}
	
}