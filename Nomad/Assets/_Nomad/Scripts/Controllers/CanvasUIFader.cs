using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static CanvasUIFader;

public interface ICanvasUIFader
{
	void FadeNow(float alpha);

	IEnumerator FadeOutDefaultRoutine();
	IEnumerator FadeInDefaultRoutine();
	IEnumerator FadeRoutine(FadeData data);
}

public class CanvasUIFader : MonoBehaviour, ICanvasUIFader
{
	[SerializeField] private Image fadeOverlay;

	public const float DefaultFadeSeconds = 0.5f;

	public struct FadeData
	{
		public float FadeAlphaTarget;
		public float Seconds;
	}
	
	public bool IsFading { get; private set; }

	public void FadeNow(float alpha)
	{
		Color currentColor = fadeOverlay.color;
		currentColor.a = alpha;
		fadeOverlay.color = currentColor;
		fadeOverlay.enabled = alpha != 0;
	}

	public IEnumerator FadeOutDefaultRoutine()
	{
		yield return FadeRoutine(new() { FadeAlphaTarget = 1, Seconds = DefaultFadeSeconds });
	}

	public IEnumerator FadeInDefaultRoutine()
	{
		yield return FadeRoutine(new() { FadeAlphaTarget = 0, Seconds = DefaultFadeSeconds });
	}

	public IEnumerator FadeRoutine(FadeData data)
	{
		fadeOverlay.enabled = true; 

		float totalTime = data.Seconds;
		float timeRemaining = totalTime;

		Color startColor = fadeOverlay.color;
		Color endColor = startColor;
		endColor.a = data.FadeAlphaTarget;

		if (startColor.a == endColor.a)
		{
			Log.Fade.Warn("Requested fade alpha is no different from current fade alpha");
		}

		while (timeRemaining > 0)
		{
			yield return null;
			timeRemaining -= Time.deltaTime;
			float t = 1 - timeRemaining / totalTime;
			fadeOverlay.color = Color.Lerp(startColor, endColor, t);
		}
		yield return null;
		fadeOverlay.color = endColor;
		if (endColor.a == 0)
		{
			fadeOverlay.enabled = false;
		}
	}
}
