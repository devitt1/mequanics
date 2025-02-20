using UnityEngine;
using System.Collections;

public class AnimateVertexNoise : MonoBehaviour
{
    public Vector4 FrequenciesX = new Vector4(1f, 1f, 1f, 1f);
    public Vector4 FrequenciesY = new Vector4(1f, 1f, 1f, 1f);

	// Update is called once per frame
	void Update ()
	{
        float t = Time.time;
        renderer.material.SetVector("_ValuesX", new Vector4(FrequenciesX.x, FrequenciesX.y, FrequenciesX.z, FrequenciesX.w * t));
        renderer.material.SetVector("_ValuesY", new Vector4(FrequenciesY.x, FrequenciesY.y, FrequenciesY.z, FrequenciesY.w * t));
	}
}
