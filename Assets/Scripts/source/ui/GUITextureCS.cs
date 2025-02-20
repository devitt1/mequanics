using UnityEngine;




public class GUITextureCS : MonoBehaviour
{
	protected VectorInt2 m_resScreen;
	protected VectorInt2 m_resImage;
	
	public Vector2 m_anchor;
	
	//
	//
	
	public void Start() 
	{	
		UpdateResolution();
	}
	
	void Update() 
	{	
		if (m_resScreen.x != Screen.width || m_resScreen.y != Screen.height){
			UpdateResolution();
		}
	}
	
	protected void UpdateResolution()
	{
		m_resScreen = new VectorInt2(Screen.width, Screen.height);
	
		Rect rect = guiTexture.pixelInset;
		rect.x = (m_resScreen.x - rect.width) / 2f;
		rect.y = (m_resScreen.y - rect.height) / 2f;
		this.guiTexture.pixelInset = rect;

	}
	
}