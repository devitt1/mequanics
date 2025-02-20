using UnityEngine;
using System.Collections;

public class Mask2CS : MonoBehaviour 
{
	static float s_epsilonRadius = 0.1f;
	static float s_radiusLarge = 1000f;
		
	static Vector3 s_screenSize = new Vector3(960f, 600f, 1f);
	Vector3 m_position = Vector3.zero;
	GameObject m_go;
	float m_radius = s_radiusLarge;
	
	// Use this for initialization
	void Start () 
	{
		// calculate 
	}
	
	// Update is called once per frame
	void Update () 
	{
		// calculate position in pixel coordinates
		if (m_go != null){
			m_position = m_go.transform.position;
		}
		
		Vector4 posScreen = Camera.mainCamera.WorldToScreenPoint(m_position);
		this.renderer.material.SetVector("_posCenter", posScreen);
		
		float r = m_radius * Camera.mainCamera.near / (Vector3.Distance(m_position, Camera.mainCamera.transform.position)) * (1.2f * s_screenSize.y);
		this.renderer.material.SetFloat("_fRadius", r);
	}
	
	public void SetGameObject(GameObject go)
	{
		this.m_go = go;
		Update();
	}
	
	public void SetCenter(Vector3 pos)
	{
		m_position = pos;
		Update();
	}
	
	
	public void SetRadius(float r)
	{
		m_radius = r;
		Update();
	}
	
	public void SetGoOfInterest(GameObject go, float radius)
	{
		SetGameObject(go);
		SetRadius(radius);
	}
	
	public void SetAreaOfInterest(Vector3 pos, float radius)
	{
		SetCenter(pos);
		SetRadius(radius);
	}
	
	public void SetAreaOfInterestSmooth(Vector3 pos, float radius, float seconds)
	{
		StartCoroutine(TransitCR(pos, radius, seconds));
	}
		
	public void SetGoOfInterestSmooth(GameObject go, float radius, float seconds)
	{
		StartCoroutine(TransitCR(go, radius, seconds));
	}
		
	
	protected IEnumerator TransitCR(Vector3 positionTarget, float radiusTarget, float seconds)
	{	
		float secRemaining = seconds;
		
		float radiusStart = m_radius;
		Vector3 positionStart = m_position;
		
		float radiusDeltaPerSec = (radiusTarget-radiusStart)/seconds;
		Vector3 positionDeltaPerSec = (positionTarget-positionStart)/seconds;
		while (secRemaining > 0){
			SetAreaOfInterest(
				positionTarget - positionDeltaPerSec * secRemaining,
				radiusTarget - radiusDeltaPerSec * secRemaining);
			secRemaining -= Time.deltaTime;
			yield return true;
		}
		SetAreaOfInterest(positionTarget, radiusTarget);
	}
	
	protected IEnumerator TransitCR(GameObject go, float radiusTarget, float seconds)
	{	
		Vector3 positionTarget = go.transform.position;
		float secRemaining = seconds;
		
		float radiusStart = m_radius;
		Vector3 positionStart = m_position;
		
		float radiusDeltaPerSec = (radiusTarget-radiusStart)/seconds;
		Vector3 positionDeltaPerSec = (positionTarget-positionStart)/seconds;
		while (secRemaining > 0){
			SetAreaOfInterest(
				positionTarget - positionDeltaPerSec * secRemaining,
				radiusTarget - radiusDeltaPerSec * secRemaining);
			secRemaining -= Time.deltaTime;
			yield return true;
		}
		SetAreaOfInterest(positionTarget, radiusTarget);
	}
}
