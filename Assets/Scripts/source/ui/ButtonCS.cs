using UnityEngine;
using System.Collections;



public class ButtonCS : MonoBehaviour 
{
	protected static AudioClip s_seClickedSuccess;
	protected static AudioClip SeClickedSuccess { get {
			if (s_seClickedSuccess == null){
				s_seClickedSuccess = (AudioClip)Resources.Load("Sounds/Gui/beep-25_doubleBeep");
			}
			return s_seClickedSuccess;
			
		} }
	
	protected CallbackParameterless m_callback;
	public CallbackParameterless Callback { set { m_callback = value; } }
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	void OnMouseDown()
	{
		SoundHubCS.Instance.PlaySE(ButtonCS.SeClickedSuccess);
		
		if (m_callback != null){
			m_callback();
		}
	}
}
