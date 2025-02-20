using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public enum IoLoc
{
	LOCAL,
	WEB,
	BUNDLED, // resources in Resouces directory
	ASSETBUNDLE
}

public class SaveLoad : MonoBehaviour
{
	//
	//
	
	protected static SaveLoad s_instance;
	public static SaveLoad Instance { 
		get { 
			if (s_instance == null){
				GameObject go = GameObject.FindGameObjectWithTag("io");
				if (go != null){
					s_instance = (SaveLoad) go.GetComponent<SaveLoad>();
				}
			}
			return s_instance; 
		}
	}
	
	protected static List<byte[]> s_savegamesVolatile;
	
	//
	//
	
	static SaveLoad()
	{
		s_savegamesVolatile = new List<byte[]> { null, null, null, null, null, null, null, null, null, null };
	}
	
	public delegate bool LoadTextCB(string uri, string buffer);
	
	public IEnumerator LoadFileTextCR(string path, IoLoc location, LoadTextCB callback)
	{
		switch (location){
		case IoLoc.WEB:
			yield return AsyncCS.Instance.StartCoroutine(LoadWebCR(path, callback));
			break;
#if ! UNITY_WEBPLAYER
		case IoLoc.LOCAL:
			yield return AsyncCS.Instance.StartCoroutine(LoadLocalCR(path, callback));
			break;
#endif
		case IoLoc.BUNDLED:
			yield return AsyncCS.Instance.StartCoroutine(LoadBundledCR(path, callback));
			break;
		default:
			break;
		}
		
		yield break;
	}
	
	public void LoadFileText(string path, IoLoc location, LoadTextCB callback)
	{
		switch (location){
		case IoLoc.WEB:
			StartCoroutine(LoadWebCR(path, callback));
			break;		
#if ! UNITY_WEBPLAYER
		case IoLoc.LOCAL:
			StartCoroutine(LoadLocalCR(path, callback));
			break;
#endif
		case IoLoc.BUNDLED:
			LoadBundled(path, callback);
			break;
		default:
			break;
		}
	}
	
	protected IEnumerator LoadWebCR(string url, LoadTextCB callback)
	{
		var www = new WWW(url);
		
		while (!www.isDone){
			yield return new WaitForSeconds(0.25f);
		}
		
		if (www.error == null) {
			callback(url, www.text);
//			UnityEngine.Debug.Log("Successfully fetched web-based resource!");
			return false;
		} else {
//			UnityEngine.Debug.ErrorLog("failed to fetch web-based resource!");
//			Popups.GenerateError(www.error, 5.0f);
			Popups.GenerateError(www.error);
			return false;
		} 
	}
	
#if ! UNITY_WEBPLAYER
	protected IEnumerator LoadLocalCR(string path, LoadTextCB callback)
	{
		string source = null;
		if (System.IO.File.Exists(path)) {
			StreamReader fs = new StreamReader(path);
			source = fs.ReadToEnd();
		}
		
		yield return new WaitForSeconds(0.1f); // this is only to make callback not to be called immediately. To be adjusted for true async loading
		
		callback(path, source);
	}
#endif
	
	protected bool LoadBundled(string path, LoadTextCB callback)
	{	
		TextAsset ta = (TextAsset)Resources.Load(path, typeof(UnityEngine.TextAsset));
		
		if (ta != null && ta.text != ""){
			callback(path, ta.text);
			return true;
		} else {
			Popups.GenerateError("Failed to load bundled circuit '" + System.IO.Path.GetFileName(path) + "'");
			return false;
		}
	}
	
	protected IEnumerator LoadBundledCR(string path, LoadTextCB callback)
	{	
		TextAsset ta = (TextAsset)Resources.Load(path, typeof(UnityEngine.TextAsset));
		
		if (ta != null && ta.text != ""){
			callback(path, ta.text);
		} else {
			Popups.GenerateError("Failed to load bundled circuit '" + System.IO.Path.GetFileName(path) + "'");
		}
		yield break;
	}
	
	public void SetSaveData(int index, byte[] buffer)
	{
		s_savegamesVolatile[index] = buffer;	
	}
	
	public byte[] GetSaveData(int index)
	{
		return s_savegamesVolatile[index];
	}
	
}

