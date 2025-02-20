using UnityEngine;
using System;

using ui;


//public delegate void CallbackParameterless();

/**
 * 
 * */
public class GuiDialogCS : GuiCS
{
	public static int s_widthText = 300; // width of text field in pixels
	
	protected float m_duration = -1;
	public float Duration { set { m_duration = value; } }
	protected string m_message = "";
	public string Message { set { m_message = value; } }
	protected string m_title = "";
	public string Title { set { m_title = value; } }
	protected HudPos m_hudPos = HudPos.CENTER;
	public HudPos Pos { set { m_hudPos = value; } }
	
	protected Rect m_rectBox;
	protected Rect m_rectText;
	protected bool m_showButton;
	protected Rect m_rectButtonOk;
	
	protected bool m_done;
	protected CallbackParameterless m_callBack;
	public CallbackParameterless Callback { set { m_callBack = value; } }
	
	//
	//
	
	// Use this for initialization
	public void Start() 
	{
		m_done = false;
		m_showButton = (m_duration == -1 ? true : false);
		
		// determine popup dimensions
		// 
		string[] linesOrig = m_message.Split('\n');
		int nLines = linesOrig.Length;
		foreach (var line in linesOrig){
			float d = line.Length / 40.0f;
			nLines += (int)(Math.Ceiling(d) - 1);
		}
		
		VectorInt2 center = new VectorInt2(Screen.width, Screen.height) / 2;
		switch (m_hudPos){
		case HudPos.BOTTOMRIGHT:
			center = (center + new VectorInt2(Screen.width, Screen.height) * 1) / 2;
			break;
		case HudPos.BOTTOMLEFT:
			center = (center + new VectorInt2(0, Screen.height) * 1) / 2;
			break;
		case HudPos.TOPRIGHT:
			center = (center + new VectorInt2(Screen.width, 0) * 1) / 2;
			break;
		case HudPos.TOPLEFT:
			center = (center + new VectorInt2(0, 0) * 1) / 2;
			break;
		case HudPos.CENTER:
			break;
		default:
			break;
		}
		int widthText = s_widthText;
		if (nLines == 1){
			widthText = System.Math.Min(s_widthText, widthText * m_message.Length / 40);
		}
			
		VectorInt2 sizeText = new VectorInt2(widthText, nLines * 22);
		VectorInt2 sizeBox = sizeText + new VectorInt2(GuiCS.SizeSpacing*2, GuiCS.SizeSpacing*3 + GuiCS.SizeTextY);
		if (m_showButton){
			sizeBox = sizeBox + new VectorInt2(0, GuiCS.SizeButtonY + GuiCS.SizeSpacing);
		}
		m_rectBox = new Rect(center.x - sizeBox.x/2 , center.y - sizeBox.y/2 ,sizeBox.x, sizeBox.y);
		m_rectText = new Rect(m_rectBox.x + GuiCS.SizeSpacing, m_rectBox.y + GuiCS.SizeSpacing*2 + GuiCS.SizeTextY, sizeText.x, sizeText.y);
		if (m_showButton){
			m_rectButtonOk = m_rectText;
			m_rectButtonOk.y = m_rectText.y + m_rectText.height + GuiCS.SizeSpacing;
			m_rectButtonOk.width = m_rectText.width;
			m_rectButtonOk.height = GuiCS.SizeButtonY;
		}
	}
	
	public void Update () 
	{
		if (!m_showButton){
			if (m_duration > 0){
				m_duration -= Time.deltaTime;
				m_duration = System.Math.Max (0, m_duration);
			}
			if (m_duration == 0){
				m_done = true;
			}
		} 
		
		if (m_done){
			if (m_callBack != null){ //TODO: Check (somehow) if m_callback is still valid. m_callBack could have become invalid since GuiDialogCS is persistent but callback objects might not be.
				m_callBack();
			}
			GameObject.Destroy(this.gameObject);
		}
		
	}
	
	void OnGUI () {
		if (!m_done)
		{	
			// Background box
			GUI.Box(m_rectBox, m_title);
			
			// Message display
			GUI.Box(m_rectText, "");
			GUI.Label(m_rectText, m_message);
			
			// Confirmation button
			if (m_showButton){
				if (ButtonWithSound(this.m_rectButtonOk, Locale.Str(PID.BN_OK))){
					m_done = true;
				}
			}
		}

	}
	
}


