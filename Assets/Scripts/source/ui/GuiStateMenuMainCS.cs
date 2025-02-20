using UnityEngine;
using System.Collections;

using ui;

/**
 * 
 * */
public class GuiStateMenuMainCS : GuiCS
{
	protected static Color s_colorBG = new Color(0.05f, 0.05f, 0.05f, 1f);
	protected static Color s_alpha0 = new Color(1f, 1f, 1f, 0f);
	protected static Color s_alpha1 = new Color(1f, 1f, 1f, 1f);
	
	protected static float s_timePerLogo = 2.0f;
	protected Color m_colorGUI;
	//
	
	public override void Start() 
	{
		GUITexture bg = transform.FindChild("splashBG").GetComponent<GUITexture>();
		
		// display splash screen background
		bg.gameObject.SetActive(true);
		m_colorGUI = s_alpha0;
	}
	
	public void AbortSplash()
	{
		GUITexture bg = transform.FindChild("splashBG").GetComponent<GUITexture>();
		bg.gameObject.SetActive(false);
		m_colorGUI = s_alpha1;
	}
	
	void Update() 
	{	
	}
	
	public void OnGUI () 
	{
		GUI.skin = this.Skin;
		
		GUI.color = m_colorGUI;
		GUI.skin.font = (Font)Resources.Load("Fonts/arial", typeof(Font));
		
		// Create a background box
		GUI.Box(new Rect(s_sizeSpacing, s_sizeSpacing, s_sizeButtonX + 3*s_sizeSpacing, s_sizeTextY + 3*s_sizeButtonY + 4*s_sizeSpacing), "");
		
		GUI.Label(new Rect(2*s_sizeSpacing, 2*s_sizeSpacing, s_sizeButtonX, s_sizeTextY + 2*s_sizeSpacing), Locale.Str(PID.MainMenu));
		
		// Create the Start button. 
		if(ButtonWithSound(new Rect(2*s_sizeSpacing,2*s_sizeSpacing + s_sizeTextY,s_sizeButtonX,s_sizeButtonY), Locale.Str(PID.BN_START))) {
			StateMachine.Instance.RequestStateChange(EState.MENUCIRCUITSELECT);
		}
		
		// Create the Tutorial button.
		if(ButtonWithSound(new Rect(2*s_sizeSpacing, 3*s_sizeSpacing + s_sizeTextY + s_sizeButtonY, s_sizeButtonX,s_sizeButtonY), Locale.Str(PID.PlayTutorial))) {
			StateMachine.Instance.RequestStateChange(EState.PLAYTUTORIAL);
		}
		
		// Create the Quit button.
		if(ButtonWithSound(new Rect(2*s_sizeSpacing, 4*s_sizeSpacing + s_sizeTextY + 2*s_sizeButtonY, s_sizeButtonX,s_sizeButtonY), Locale.Str(PID.BN_QUIT))) {
			StateMachine.Instance.RequestStateChange(EState.TERMINATE);
		}
	}
	
	public void RunSplash(Texture2D[] logos)
	{
		StartCoroutine(SplashCR(logos));	
	}
	
	protected IEnumerator SplashCR(Texture2D[] logos)
	{
		// no splash without logo
		int nLogos = logos.GetLength(0);
		if (nLogos == 0){ yield break; }
		
		// background 
		GUITexture bg = transform.FindChild("splashBG").GetComponent<GUITexture>();
		bg.gameObject.SetActive(true);
		bg.color = s_colorBG;
		m_colorGUI = s_alpha0;
		
		// generate logos
		GUITexture[] texLogos = new GUITexture[nLogos];
		for (int i = 0; i < nLogos; i++){
			GUITexture tex = new GameObject("logo").AddComponent<GUITexture>();
			tex.transform.position =  new Vector3(0, 0, 2f);
			tex.transform.localScale = Vector3.zero;
			tex.texture = logos[i];
			tex.pixelInset = new Rect(Screen.width/3, Screen.height/3, Screen.width/3, Screen.height/3);
			tex.gameObject.SetActive(false);
			texLogos[i] = tex;
		}
	
		// display logos one at a time
		float duration;
		float durRemaining;
		foreach (GUITexture tex in texLogos){
			tex.color = s_alpha0;
			tex.gameObject.SetActive(true);
			duration = s_timePerLogo * 0.25f;
			durRemaining = duration; 
			while (durRemaining > 0){
				tex.color = Color.Lerp(s_alpha0, s_alpha1, 1f - durRemaining/duration);
				yield return true;
				durRemaining -= Time.deltaTime;
			}
			tex.color = s_alpha1;
			duration = s_timePerLogo * 0.5f;
			durRemaining = duration; 
			while (durRemaining > 0){
				yield return true;
				durRemaining -= Time.deltaTime;
			}
			duration = s_timePerLogo * 0.25f;
			durRemaining = duration; 
			while (durRemaining > 0){
				tex.color = Color.Lerp(s_alpha1, s_alpha0, 1f - durRemaining/duration);
				yield return true;
				durRemaining -= Time.deltaTime;
			}
			tex.gameObject.SetActive(false);
			
//			yield return new UnityEngine.WaitForSeconds(0.5f);
		}
		
		// fade out background
		duration = 2.0f;
		durRemaining = duration; 
		Color c = s_colorBG;
		while (durRemaining > 0){
			c.a = durRemaining/duration/2;
			bg.color = c;
			yield return true;
			durRemaining -= Time.deltaTime;
			
		}
		bg.gameObject.SetActive(false);
		
		// fade in GUI
		duration = 0.5f;
		durRemaining = duration; 
		while (durRemaining > 0){
			m_colorGUI = Color.Lerp(s_alpha0, s_alpha1, 1f - durRemaining/duration);
			yield return true;
			durRemaining -= Time.deltaTime;
		}
		m_colorGUI = s_alpha1;
		
		// dispose of logos and the background
		foreach (var tex in texLogos){
			GameObject.Destroy(tex.gameObject);
		}
	}
	
}