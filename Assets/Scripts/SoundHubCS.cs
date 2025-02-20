using UnityEngine;
using System.Collections;

public class SoundHubCS : MonoBehaviour 
{
	protected static SoundHubCS s_instance;
	public static SoundHubCS Instance {
		get { return s_instance; }
	}
	
	//
	//
	
	void OnEnable()
	{
		if (SoundHubCS.s_instance == null){
			s_instance = this;
		} else {
			System.Diagnostics.Debug.Assert(false);
		}
	}
	
	void OnDisable()
	{
		SoundHubCS.s_instance = null;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void PlaySE(AudioClip clip)
	{
		audio.PlayOneShot(clip, 1f);
	}
}
