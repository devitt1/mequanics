using UnityEngine;
using System.Collections;

public class MaskCS : MonoBehaviour 
{
	protected Vector3 m_posHighlight;
	public float m_radius;
	public const float DistCam = 2f;	
	public static float s_scalePerDistCam = DistCam * 0.5f * Mathf.Sqrt(3f);
	
	// Uwwse this for initialization
	void Start() 
	{
		m_posHighlight = Vector3.zero;
		m_radius = 1f;
		
		this.transform.parent = Camera.mainCamera.transform;
		this.transform.rotation = Quaternion.identity;
		this.transform.localPosition = new Vector3(0,0, DistCam);
		this.gameObject.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 posViewport = Camera.mainCamera.WorldToViewportPoint(m_posHighlight);
		transform.localPosition = new Vector3(posViewport.x, posViewport.y, DistCam);
		float scale = m_radius * 2f * s_scalePerDistCam;
		transform.localScale = new Vector3(scale, scale, scale);
	}
	
	public void SetAreaOfInterest(Vector3 posOfInterest, float radius)
	{
		m_posHighlight = posOfInterest;
		m_radius = radius;
	}
}
