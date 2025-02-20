using UnityEngine;
using System.Collections.Generic;
using System.Linq;



/**
 * 	
 * */
public class Achievements
{
	public class Threshold 
	{
		public int score;
		public bool surpassed;
		public BoundingBoxCS bb;
		
		public Threshold(int score)
		{
			this.score = score;
			this.surpassed = false;
		}
	}
	
	//
	//
	
	protected static float s_bbPaddingWidth = 0.1f;
	protected static Vector3 s_bbPadding = new Vector3(s_bbPaddingWidth, s_bbPaddingWidth, s_bbPaddingWidth);
	
	protected Game m_game;
	protected LinkedList<Threshold> m_thresholds;
	protected BoundingBoxCS m_bbCircuit;
	
	//
	//
	
	public Achievements(Game game)
	{
		m_game = game;
		
		m_thresholds = new LinkedList<Threshold>();
		
		m_bbCircuit = PrefabHubCS.Instance.GetBoundingBox();
		m_bbCircuit.gameObject.SetActive(false);
	}
	
	// this should be called after a new circuit was loaded
	public void SetThresholds(int[] scores)
	{
		DiscardThresholds();
		
		List<int> iScores = scores.ToList();
		iScores.Sort();
		
		foreach(int score in iScores){
			Threshold t = new Threshold(score);
			t.bb = PrefabHubCS.Instance.GetBoundingBox();
			t.bb.gameObject.SetActive(false);
			t.bb.Size = Score.SizeFromScore(score);
			m_thresholds.AddFirst(t);
		}
		
		Update();
	}
	
	public void Update()
	{
		UpdateVisual();
	}
	
	protected void UpdateVisual()
	{
		// Retrieve score and circuit dimensions from game 
		int scoreCurrent = m_game.Score1;
		var circuitBB = m_game.CircuitBB;
		Vector3 center = (circuitBB.Key + circuitBB.Value)/2;
		Vector3 sizeBBCir = circuitBB.Value - circuitBB.Key;
		
		// apply to visual bounding box
		m_bbCircuit.transform.position = center;
		m_bbCircuit.transform.localScale = sizeBBCir + 2*s_bbPadding;
		m_bbCircuit.renderer.material.SetColor("_TintColor", new Color(1f, 1f, 1f, 16f/255f)); // set to white 
		m_bbCircuit.gameObject.SetActive(true);
		
		// Update thresholds surpassed 
		// notify gui of stars to display
		int stars = 0;
		foreach(var t in m_thresholds){
			if (!t.surpassed){
				t.surpassed = scoreCurrent <= t.score;
				if (t.surpassed){
					stars++;
					m_game.AchievementChanged(stars);
				} else {
					break;
				}
			} else {
				stars++;
			}
		}
			
		// Adjust bounding box for next threshold to reach
		bool found = false;
		foreach(var t in m_thresholds){
			if(t.surpassed || found) {
				t.bb.gameObject.SetActive(false);
			} else {
				found = true;
				t.bb.transform.position = center;
				// have threshold BB height match circuit BB height
				Vector3 sizeBBThres = sizeBBCir;
				if (sizeBBCir.x > sizeBBCir.z){
					sizeBBThres.x = t.score - sizeBBCir.y - sizeBBCir.z;
					if (sizeBBThres.x < sizeBBThres.z){
						float lenEdge = (sizeBBThres.x + sizeBBThres.z)/2f;
						sizeBBThres.z = Mathf.CeilToInt(lenEdge/2)*2;
						sizeBBThres.x = Mathf.FloorToInt(lenEdge/2)*2;
					}
				} else {
					sizeBBThres.z = t.score - sizeBBCir.y - sizeBBCir.x;
					if (sizeBBThres.x > sizeBBThres.z){
						float lenEdge = (sizeBBThres.x + sizeBBThres.z)/2f;
						sizeBBThres.x = Mathf.CeilToInt(lenEdge/2)*2;
						sizeBBThres.z = Mathf.FloorToInt(lenEdge/2)*2;
					}
				}
				t.bb.transform.localScale = sizeBBThres + 2*s_bbPadding;
				
				t.bb.SetExitation(((float)t.score)/scoreCurrent);
				t.bb.gameObject.SetActive(true);
			}
		}
	}
	
	protected void DiscardThresholds()
	{
		foreach (Threshold t in m_thresholds){
			GameObject.Destroy(t.bb.gameObject);
		}
		m_thresholds.Clear();
	}
	
}
