using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ui;




public struct LogoData
{
	public GUITexture texture;
	public Color colorBG;
}

/**
 * 
 * */
public class SplashCS : MonoBehaviour, IEventHandler
{
	protected Color m_alpha0 = new Color(0.5f, 0.5f, 0.5f, 0f);
	protected Color m_alpha1 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	protected Color m_all0 = Vector4.zero;
	
	public float m_timePerLogo = 2.0f;
	public float m_timeBetweenLogos = 1.0f;
	
	public GUITexture m_bg;
	public GUITexture[] m_logos = new GUITexture[3];
	protected List<LogoData> m_logosData;
	
	protected Color[] m_colorsBG = new Color[]{ 
		new Color(0f, 0f, 0f, 0.5f),
		new Color(0f, 0f, 0f, 0.5f),
		new Color(0.25f, 0.25f, 0.25f, 0.5f)
	};
	
	protected bool m_running = false;
	protected VectorInt2 m_resolution;
	//
	//
	
	public void Start() 
	{
		m_logosData = new List<LogoData>();
		foreach (int i in Enumerable.Range(0, m_logos.Length))
		{
			var data = new LogoData();
			data.texture = m_logos[i];
			data.colorBG = m_colorsBG[i];
			m_logosData.Add(data);
		}
		
		UpdateResolution();
	}
	
	void Update() 
	{	
		if (m_resolution.x != Screen.width || m_resolution.y != Screen.height){
			UpdateResolution();
		}
	}
	
	protected void UpdateResolution()
	{
		m_resolution = new VectorInt2(Screen.width, Screen.height);
		
		m_bg.pixelInset = new Rect(0, 0, m_resolution.x, m_resolution.y);
		foreach (GUITexture logo in m_logos){
			Rect rect = logo.pixelInset;
			rect.x = (m_resolution.x - rect.width) / 2f;
			rect.y = (m_resolution.y - rect.height) / 2f;
			logo.pixelInset = rect;
		}
	}
	
	public void Run()
	{
		m_running = true;
		StartCoroutine(SplashCR(m_logosData));	
	}
	
	public void Abort()
	{
		if (m_running){
			StopCoroutine("splashCR");
		}
		this.gameObject.SetActive(false);
	}

	//
	//
	
	protected IEnumerator SplashCR(List<LogoData> logos)
	{
		// Hide all logos
		foreach (LogoData logo in logos)
		{
			logo.texture.color = m_alpha0;
			logo.texture.gameObject.SetActive(true);
			logo.texture.enabled = false;
		}
		
		// Init BG to black
		m_bg.gameObject.SetActive(true);
		m_bg.enabled = true;
		m_bg.color = Color.black;
		
		// iterate logos
		float duration, durRemaining;
		Color colorFrom;
		
		// display logos one at a time
		foreach (LogoData logo in logos)
		{
			// fade BG to desired color
//			yield return StartCoroutine(FadeTextureCR(m_bg, logo.colorBG, m_timeBetweenLogos));
			durRemaining = duration = m_timeBetweenLogos;
			colorFrom = m_bg.color;
			while (durRemaining > 0){
				m_bg.color = Color.Lerp (colorFrom, logo.colorBG, 1f - (durRemaining/duration));
				durRemaining -= Time.deltaTime;
				yield return true;
			}
			m_bg.color = logo.colorBG;
			
			// activate logo
			logo.texture.enabled = true;
			
			// fade in logo
//			yield return StartCoroutine(FadeTextureCR(logo.texture, m_alpha0, m_alpha1, m_timePerLogo * 2.5f));
			durRemaining = duration = m_timePerLogo * 0.25f;
			while (durRemaining > 0){
				logo.texture.color = Color.Lerp(m_alpha0, m_alpha1, 1f - durRemaining/duration);
				durRemaining -= Time.deltaTime;
				yield return true;
			}
			logo.texture.color = m_alpha1;
			
			// display logo
//			yield return new WaitForSeconds(m_timePerLogo * 0.5f);
			durRemaining = duration = m_timePerLogo * 0.5f;
			while (durRemaining > 0){
				durRemaining -= Time.deltaTime;
				yield return true;
			}
			
			// fade out logo
//			yield return StartCoroutine(FadeTextureCR(logo.texture, m_alpha1, m_alpha0, m_timePerLogo * 2.5f));
			durRemaining = duration = m_timePerLogo * 0.25f;
			while (durRemaining > 0){
				logo.texture.color = Color.Lerp(m_alpha1, m_alpha0, 1f - durRemaining/duration);
				durRemaining -= Time.deltaTime;
				yield return true;
			}
			logo.texture.color = m_alpha0;
			
			// Deactivate logo
			logo.texture.enabled = false;
		}
		
		// fade out background
//		yield return StartCoroutine(FadeTextureCR(m_bg, m_all0, 2.0f));
		durRemaining = duration = 2.0f;
		colorFrom = m_bg.color;
		Color colorTo = m_bg.color;
		colorTo.a = 0;
		while (durRemaining > 0){
			m_bg.color = Color.Lerp (colorFrom, colorTo, 1f - (durRemaining/duration));
			durRemaining -= Time.deltaTime;
			yield return true;
		}
		m_bg.color = m_alpha0;
		
		m_bg.enabled = false;
		
		this.gameObject.SetActive(false);
		
		m_running = false;
	}
	
	
//	protected IEnumerator FadeTextureCR(GUITexture tex, Color colorTo, float duration)
//	{
//		Color colorFrom = tex.color;
//		float timeStart = Time.time;
//		float durRemaining = duration;
//		while (durRemaining > 0){
//			tex.color = Color.Lerp(colorFrom, colorTo, 1f - durRemaining/duration);
//			yield return true;
//			durRemaining = Time.time - timeStart;
//		}
//		tex.color = colorTo;
//	}
//	
//	protected IEnumerator FadeTextureCR(GUITexture tex, Color colorFrom, Color colorTo, float duration)
//	{
//		float timeStart = Time.time;
//		float durRemaining = duration;
//		while (durRemaining > 0){
//			tex.color = Color.Lerp(colorFrom, colorTo, 1f - durRemaining/duration);
//			yield return true;
//			durRemaining = Time.time - timeStart;
//		}
//		tex.color = colorTo;
//	}
	
	public bool HandleEvent(UnityEngine.Event e)
	{		
		return false;
	}
	
	
}