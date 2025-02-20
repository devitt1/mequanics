
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ui;

// callback function prototype
//public delegate void ButtonCB(KeyCode keycode);

/**
 * 	
 * */
public class Gui3DStatePlayCS : GuiCS, IEventHandler
{
	public new StatePlay State{
		set{ m_state = value; }
		get{ return (StatePlay) this.m_state; }
	}
	
	//
	// variables
	
	public Collider m_bnBack;
	public Collider m_bnHelp;
	public TextMesh m_labelCircuitName;
	public TextMesh m_labelScore;
	public TextMesh m_labelAction;
	public Collider[] m_bnColliders = new Collider[8];
	public StarCS[] m_stars = new StarCS[3];
	
	protected Dictionary<string, ActionButtonCS> m_actionButtons = new Dictionary<string, ActionButtonCS>();
	
	protected int m_score1 = 0;
	public int Score1 { set { 
			m_score1 = value; 
			if (m_score1 != null) { 
				m_labelScore.text = m_score1.ToString(); 
			} 
		} 
	}
	
	public string CircuitName { set { if (m_labelCircuitName!=null) { m_labelCircuitName.text = value; } } }
	
	protected int m_score2 = 0;
	public int Score2 { set { m_score2 = value; } }
	
	protected int m_numStars = 0;
	public int Stars { set { 
			// activate another star
			foreach (int i in Enumerable.Range(0, 3)){
				if (value > i) {
					m_stars[i].gameObject.SetActive(true);
				} else {
					m_stars[i].gameObject.SetActive(false);
				}
			}

		} 
	}
	
	
	bool m_dragging;
	Rect m_rectDrag;
	Vector3 m_posDragSelectBegin;
	Vector3 m_posDragSelectEnd;
	Material m_matDragBox;
	
	// override methods
	
	static Gui3DStatePlayCS()
	{
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
	}
	
	public override void Start()
	{
	}
	
	public void Cleanup()
	{
		this.ResetStars();
			
		this.m_actionButtons.Clear();
	}
	
	
	public bool HandleEvent(UnityEngine.Event e)
	{		
		switch (e.type){
		case EventType.MouseDown:
			if (e.button != 0){
				return false;
			}
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("gui3D"))){
				if (hit.collider == this.m_bnBack){
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
					StateMachine.Instance.RequestStateChange(EState.MENUCIRCUITSELECT);
				} else if (hit.collider == this.m_bnHelp) {
					SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
					Popups.Instance.CreateDialog(Locale.Str(PID.HelpString), "Help");
				} else {
					foreach (Collider c in m_bnColliders){
						if (hit.collider == c)
						{
							SoundHubCS.Instance.PlaySE(GuiCS.SoundClickSuccessful);
							ActionButtonCS ab = c.transform.GetComponent<ActionButtonCS>();
							ab.m_callback(ab.m_keycode);
							if (ab.GetType() == typeof(ActionButtonToggleCS)){
								((ActionButtonToggleCS)ab).Activate();
							}
							break;
						}
					}
				}
				return true;
			}
			break;
		case EventType.MouseUp:
			if (e.button != 0){
				return false;
			}
			RaycastHit hit2 = new RaycastHit();
			Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray2, out hit2, 100000.0f, 1 << LayerMask.NameToLayer("gui3D"))){
				return true;
			}
			break;
		default:
			break;
		}
		return false;
	}
	
	public void OnGUI () 
	{	
	}
		
	//
	// common methods
	
	public void RegisterActionButton(string label, ButtonCB callback, KeyCode keycode, bool enabled)
	{
		RegisterActionButton(label, "", callback, keycode, enabled);
	}
		
	public void RegisterActionButton(string label, string hotkey, ButtonCB callback, KeyCode keycode, bool enabled)
	{
		RegisterActionButton(label, hotkey,  callback,  keycode, enabled, null);
	}	
	
	public void RegisterActionButton(string label, ButtonCB callback, KeyCode keycode, bool enabled, ButtonCB callbackRelease)
	{
		RegisterActionButton(label, "", callback, keycode, enabled, callbackRelease);
	}
		
	public void RegisterActionButton(string label, string hotkey, ButtonCB callback, KeyCode keycode, bool enabled, ButtonCB callbackRelease)
	{
		int i = m_actionButtons.Count;
		string strHotKey = hotkey != "" ? hotkey : keycode.ToString();
		
		ActionButtonCS ab;
		if (callbackRelease == null){
			ab = m_bnColliders[i].transform.GetComponent<ActionButtonCS>();
		} else {
			GameObject go = m_bnColliders[i].gameObject;
			ab = go.GetComponent<ActionButtonToggleCS>();
			((ActionButtonToggleCS)ab).m_callbackReleased = callbackRelease;
		}
		ab.m_label = label;
		ab.m_hotkey = hotkey;
		ab.m_keycode = keycode;
		ab.enabled = enabled;
		ab.m_callback = callback;
		ab.m_index = i;
		ab.m_coll = m_bnColliders[i];
		
		m_actionButtons[ab.m_label] =  ab;
	}
		
	public void EnableButton(string label, bool enabled)
	{
		if (m_actionButtons.ContainsKey(label)) {
			m_actionButtons[label].m_enabled = enabled;
		}
	}
	
	//
	// score display
		
	public void SetScores(int score1, int score2, bool inj, bool stable)
	{	
		Score1 = score1;
		Score2 = score2;
//		Stable = stable;
//		InjectionOk = inj;
	}
	
	// drag selection rectangle
	
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
	
	/**
	 * Renders the drag selection box
	 * */
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
	
	public void SetActionString(string text)
	{
		m_labelAction.text = text;
	}
	
	public string ActionString { get { return m_labelAction.text; } }
	
	protected void ResetStars()
	{
		foreach (int i in Enumerable.Range(0, 3)){
			m_stars[i].gameObject.SetActive(false);
		}
	}
	
	public void ReleaseDuplicateMode()
	{
		ActionButtonCS abCS = this.m_actionButtons["Switching"];
		((ActionButtonToggleCS)abCS).Deactivate();
	}
}