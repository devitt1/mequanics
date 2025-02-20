using UnityEngine;
using System.Collections;

public class ActivityIndicatorCS : MonoBehaviour
{
	protected const float s_period = 1.0f;
	protected float m_current;
	protected int m_phase;
	
	protected static string[] s_phases = {"Loading ", "Loading .", "Loading ..", "Loading ..."};
	
	// Use this for initialization
	void Start () 
	{
		m_current = 0;
		m_phase = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		m_current += Time.deltaTime;
		if (m_current > s_period) {
			m_current -= s_period;
		}
	}
	
	void OnGUI () 
	{	
		// draw activity indicator
		GUI.skin.font = (Font)Resources.Load("Fonts/arial", typeof(Font));
		int phase = (int) ((m_current/s_period) * (s_phases.GetLength(0) - 1));
		string label = s_phases[phase]; 
		GUI.Label(new Rect(Screen.width/2, Screen.height/2, 120, 40), label);
		
	}
}
