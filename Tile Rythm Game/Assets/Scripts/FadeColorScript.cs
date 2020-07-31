using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeColorScript : MonoBehaviour {

	// Reference to game objects Sprite Renderer component
	SpriteRenderer sprite;

	// Variable to hold fading speed
	private float t = 0f;
	private float duration = 1f;
	private bool startFade;

	Color newColor, oldColor;

	// Use this for initialization
	void Start () {

		// Getting Sprite Renderer component
		sprite = GetComponent<SpriteRenderer> ();

		newColor = Color.black;
		oldColor = sprite.color;

		startFade = false;
		
	}

	void Update()
	{
		if (startFade)
		{
			sprite.color = Color.Lerp(oldColor, newColor, t);

			if (t < 1)
			{
				t += Time.deltaTime/duration;
			}else{
				startFade = false;
			}
		}
	}

	// Method that starts fading coroutine when UI button is pressed
	public void StartFade(float dur, string oldCol, string newCol)
	{	
		duration = dur;
		ColorUtility.TryParseHtmlString(oldCol, out oldColor);
		ColorUtility.TryParseHtmlString(newCol, out newColor);
		t = 0;
		startFade = true;
	}

	// Cancel fading to red
	public void StopFade()
	{
		startFade = false;
	}
}
