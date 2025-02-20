using UnityEngine;
using System.Collections;

/**
 * 
 * */
public class GuiMetricsCS : MonoBehaviour 
{
	public const int c_NumFramesAvg = 10;
	protected float[] m_framerates;
	protected int m_frameIndex;
	protected float m_framerateAvg;
	
	// Use this for initialization
	void Start () {
		m_framerates = new float[c_NumFramesAvg];
		m_frameIndex = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		m_framerates[m_frameIndex] = 1f/Time.smoothDeltaTime;
		m_frameIndex++;
		if (m_frameIndex >= c_NumFramesAvg){
			m_framerateAvg = 0;
			foreach (float f in m_framerates){
				m_framerateAvg += f;
			}
			m_framerateAvg /= c_NumFramesAvg;
			
			m_frameIndex = 0;
		}
	}
	
	void OnGUI () 
	{
		if (UnityEngine.Debug.isDebugBuild){
			GUI.skin.font = (Font)Resources.Load("Fonts/arial", typeof(Font));
		
			// version
			GUI.Label(new Rect(Screen.width-60, Screen.height-20, 60, 20), QConfig.s_versionComplete);
			
			// framerate
			
			GUI.Label(new Rect(0, Screen.height-20, 400, 20), "Frames/s: " + (m_framerateAvg).ToString("0.00"));
		
			// runtime platform
			GUI.Label(new Rect(0, Screen.height-40, 400, 20), Application.platform.ToString());
		}
	}
	
}
