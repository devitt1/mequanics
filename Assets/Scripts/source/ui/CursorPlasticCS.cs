using UnityEngine;
using System.Collections;
using System.Linq;

using tools;

/**
 * 
 * */
public class CursorPlasticCS : MonoBehaviour 
{
	protected bool m_aboutToShow;
	public ArrowCS[] m_arrows = new ArrowCS[6];
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	public void SetAxes(Axis active, Axis normal)
	{
		SetAxes(active);
	}
		
	public void SetAxes(Axis active)
	{	
		// select texture
		if (active == Axis.INVALID){
			foreach (ArrowCS v in m_arrows){
				v.SetScale(1f);
			}
		} else {
			foreach (ArrowCS v in m_arrows){
				if (Tools.GetAxisFromDirection(v.dir) == active){
					v.SetScale(1.5f);
				} else {
					v.SetScale(0.75f);
				}
			}
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
			foreach (ArrowCS v in m_arrows){
				v.gameObject.SetActive(show);
			}
		}
	}
}
