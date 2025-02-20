//// [
//// THIS FILES MAKES PART OF THE 3DFEEL PRODUCT
//// 
//// UNLESS ANOTHER COPYRIGHT NOTICE (ORIGINAL COPYRIGHT NOTICE FOLLOW
//// THIS HEADER AND CONTRADICTS THIS INFORMATION, THIS FILE HAS BEEN 
//// DEVELOPED BY LM3LABS AND MAKES PART OF 3DFEEL.
//// 
//// CONSEQUENTLY THIS FILE IS LICENSED AS PART OF 3DFEEL
//// 3DFEEL : COPYRIGHTS(C) LM3LABS 2010->2011 - ALL RIGHTS RESERVED
////
//// status : alpha
//// depends :
//// 
//// @copyright $Copyright$
//// @version $Revision: ???? $
//// @lastrevision $Date: ???? $
//// @modifiedby $LastChangedBy: ???? $
//// @lastmodified $LastChangedDate: ???? $
////     
//// ]
using UnityEngine;
using System.Collections;

public class LM3DF_Spinning : MonoBehaviour {
	
	public Vector3 spinning=Vector3.zero;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.localRotation=Quaternion.Euler(spinning.x*Time.deltaTime,
			                                     spinning.y*Time.deltaTime,
			                                     spinning.z*Time.deltaTime 
			                                    )*transform.localRotation;
	}
}


