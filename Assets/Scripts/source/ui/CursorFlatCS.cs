using UnityEngine;
using System.Collections;

using tools;

/**
 * 
 * */
public class CursorFlatCS : MonoBehaviour 
{
	protected bool m_aboutToShow;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void SetAxes(Axis active, Axis normal)
	{
		System.Diagnostics.Debug.Assert(active != normal);
		
		// select texture
		Texture t;
		if (active == Axis.INVALID){
			t = (Texture)Resources.Load("Textures/ArrowCross");
		} else {
			t = (Texture)Resources.Load("Textures/ArrowCross_X");
		}
		this.transform.GetChild(0).renderer.material.SetTexture("_MainTex", t);
		this.transform.GetChild(1).renderer.material.SetTexture("_MainTex", t);
		
		// set orientation
		switch (normal) {
		case Axis.X:
			if (active == Axis.Y){
				this.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 90f));
			} else { // Axis.Z
				this.transform.localRotation = Quaternion.Euler(new Vector3(0, 90f, 0));
			}
			break;
		case Axis.Y:
			if (active == Axis.X){
				this.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
			} else { // Axis.Z
				this.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 90f));
			}
			break;
		case Axis.Z:
			if (active == Axis.X){
				this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			} else { // Axis.Y
				this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90f));
			}
			break;
		default:
			break;
		}
	}
	
	public void Show(bool show, float delay)
	{
		m_aboutToShow = show;
		StartCoroutine(ShowCR(show, delay));
	}
	
	IEnumerator ShowCR(bool show, float delay)
	{
		if (delay > 0){
			yield return new WaitForSeconds(delay);
		}
		
		if (show == m_aboutToShow){
			this.transform.GetChild(0).gameObject.SetActive(show);
			this.transform.GetChild(1).gameObject.SetActive(show);
		}
	}
}
