using UnityEngine;
using System;

namespace ui
{
	

// callback function prototype
public delegate void ButtonCB(KeyCode keycode);

/**
 * 
 * */
public abstract class GuiCS : MonoBehaviour
{
	public static int SizeSpacing { get{ return s_sizeSpacing; } }
	public static int SizeButtonX { get{ return s_sizeButtonX; } }
	public static int SizeButtonY { get{ return s_sizeButtonY; } }
	public static int SizeTextY { get{ return s_sizeTextY; } }
		
	protected static int s_sizeSpacing = 10;
	protected static int s_sizeButtonX = 90;
	protected static int s_sizeButtonY = 20;
	protected static int s_sizeTextY = 20;
		
	public State State{ set{ m_state = value; } }
		
	protected State m_state;
	
	protected static float s_scale;
	public static float Scale {
		get { 
			if (s_scale == 0){
				if (Application.platform == RuntimePlatform.Android){
					float ppi = 216f;
					float ppcm = ppi / 2.54f;
					s_scale = ppcm / 40.0f;
				} else {				
					s_scale = 2.0f;
				}
			}
			return s_scale;
		}
	}
		
	protected GUISkin m_skin;
	public GUISkin Skin { get {
				if (m_skin == null) {
					m_skin = GameObject.FindGameObjectWithTag("skin").GetComponent<SkinCS>().Skin;
				}
				return m_skin;
			} }
		
	protected static AudioClip s_soundClickSuccessful;
	public static AudioClip SoundClickSuccessful { get { if (s_soundClickSuccessful == null) { s_soundClickSuccessful = (AudioClip)Resources.Load("Sounds/Gui/beep-25_doubleBeep"); } return s_soundClickSuccessful; } }
	protected static AudioClip s_soundClickFailed;
	public static AudioClip SoundClickFailed { get { if (s_soundClickFailed == null) { s_soundClickFailed = (AudioClip)Resources.Load("Sounds/Gui/button-24_failure"); } return s_soundClickFailed; } }
	protected static AudioClip s_soundClickSwoosh;
	public static AudioClip SoundClickSwoosh { get { if (s_soundClickSwoosh == null) { s_soundClickSwoosh = (AudioClip)Resources.Load("Sounds/Gui/button-37_pling"); } return s_soundClickSwoosh; } }
		
	//
	//
	
	static GuiCS()
	{
		s_sizeSpacing = (int)(Scale * s_sizeSpacing);
		s_sizeButtonX = (int)(Scale * s_sizeButtonX);
		s_sizeButtonY = (int)(Scale * s_sizeButtonY);
		s_sizeTextY = (int)(Scale * s_sizeTextY);	
	}
		
	public virtual void Start()
	{
	}
		
	public bool ButtonWithSound(Rect rect, string text)
	{
		bool clicked = GUI.Button(rect, text);
		if (clicked) {
			SoundHubCS.Instance.PlaySE(SoundClickSuccessful);
		}
		return clicked;
	}
		
	public bool ButtonWithSound(Rect rect, string text, GUIStyle style)
	{
		bool clicked = GUI.Button(rect, text, style);
		if (clicked) {
			SoundHubCS.Instance.PlaySE(SoundClickSuccessful);
		}
		return clicked;
	}
		
}
	

}

