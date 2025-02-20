using UnityEngine;
using System;

using ui;


public delegate void CallbackParameterless();

/**
 * 
 * */
public class GuiDialog3DCS : GuiCS
{
	protected bool m_done;
	
	protected string m_message = "";
	public string Message { set { m_message = value; } }
	protected string m_title = "";
	public string Title { set { m_title  = value; } }
	protected HudPos m_hudPos = HudPos.CENTER;
	public HudPos Pos { set { m_hudPos = value; } }
	public bool ShowButton { set { m_showButton = value; } }
	protected bool m_showButton = false;
	
	protected CallbackParameterless m_callBack;
	public CallbackParameterless Callback { set { m_callBack = value; } }
	
	public TextMesh m_meshText;
	public TextMesh m_meshTitle;
	public Collider m_collButton;
	public ButtonCS m_button;
	public GameObject m_buttonVisual;
	
	//
	//
	
	// Use this for initialization
	public void Start() 
	{
		//
		if (m_showButton) {
			m_button.Callback = this.SetDone;
		} 
		m_buttonVisual.SetActive(m_showButton);
		
		// set title
		this.m_meshTitle.text = m_title;
		
		// set text
		
		// insert line-breaks
		string textLinebroken = this.m_message; 
		int lettersPerLine = 40;
		int nLines = 1;
		int i = 0;
		while (i < textLinebroken.Length - lettersPerLine) {
			if (textLinebroken.Substring(i, lettersPerLine).Contains("\n")){
				i += textLinebroken.Substring(i).IndexOf("\n");
				i++;
			} else {
				i += lettersPerLine;
				while (textLinebroken[i] != ' '){
					i -= 1;
				}
				textLinebroken = textLinebroken.Insert(i, "\n");
				i++;
			}
			nLines++;
		}
		this.m_meshText.text = textLinebroken;
		if (nLines > 4){
			// enlarge dialog box
			int extend = (nLines > 12) ? 2 : 1;
			this.transform.Find("placeBackground").localScale = new Vector3(1f, 1f + extend*1f, 1f);
			this.transform.Find("placeTypo").Translate(0, 0.55f * extend , 0);
		}
		
		m_done = false;
	}
	
	public void Update () 
	{
//		if (!m_showButton){
//			if (m_duration > 0){
//				m_duration -= Time.deltaTime;
//				m_duration = System.Math.Max (0, m_duration);
//			}
//			if (m_duration == 0){
//				m_done = true;
//			}
//		} 
		
		if (m_done){
			if (m_callBack != null){ //TODO: Check (somehow) if m_callback is still valid. m_callBack could have become invalid since GuiDialog3DCS is persistent but callback objects might not be.
				m_callBack();
			}
			GameObject.Destroy(this.gameObject);
		}
		
	}
	
	public void SetDone()
	{
		m_done = true;
	}
	
}


