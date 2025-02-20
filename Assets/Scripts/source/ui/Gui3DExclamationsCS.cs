using UnityEngine;
using System.Collections;
using System.Linq;
using System;


/***/
public class Gui3DExclamationsCS : MonoBehaviour 
{
	public enum ItemsToSay
	{
		NICE,
		GOOD,
		WELLDONE,
		GREAT,
		AWESOME
	}
	
	public GameObject[] m_items = new GameObject[5];
	protected int m_itemLast;
	
	//
	//
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!animation.isPlaying)
		{
			m_items[m_itemLast].SetActive(false);
		}
	}
	
	public void Exclaim(ItemsToSay item)
	{
		foreach (int i in Enumerable.Range(0, 5))
		{
			if (i == (int)item){
				m_items[i].SetActive(true);
			} else {
				m_items[i].SetActive(false);
			}
		}
		m_itemLast = (int)item;
		
		this.animation.Play();
	}
	
}
