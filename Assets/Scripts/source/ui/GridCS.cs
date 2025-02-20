using UnityEngine;


/**
 * 
 * */
public class GridCS : MonoBehaviour
{
	public GridRenderer gridRenderer;
	
	//
	//
	
	void Start() {}
	
	void Update() {}
	
	public void OnRenderObject()
	{
		if (gridRenderer != null){
			gridRenderer.Draw();
		}
	}
	
}
