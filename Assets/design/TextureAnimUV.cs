using UnityEngine;
using System.Collections;

public class TextureAnimUV : MonoBehaviour {
	
	private Vector2 Offset = new Vector2(0,0);
	public Vector2 Speed = new Vector2(0.1f,0);
	
	void Start () {
	
	}
	
	void Update () {
		Offset += Speed*Time.deltaTime;
		foreach(Material mat in renderer.materials)
		{
			mat.SetTextureOffset ("_MainTex", Offset);
		}
		
	}
}
