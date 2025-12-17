using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro; // Ensure we support TMP fading if needed

/// <summary>
/// A lightweight coroutine-based animation system for UI elements.
/// Replaces the need for external tweening libraries for simple effects.
/// </summary>
public static class UIAnimator
{
    public static IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        group.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    public static IEnumerator FadeOut(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        float startAlpha = group.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }
        group.alpha = 0f;
    }

    public static IEnumerator ScaleIn(Transform target, float duration, AnimationCurve curve = null)
    {
        if (target == null) yield break;
        target.localScale = Vector3.zero;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (curve != null) t = curve.Evaluate(t);
            target.localScale = Vector3.one * t;
            yield return null;
        }
        target.localScale = Vector3.one;
    }
    
    public static IEnumerator Pulse(Transform target, float duration, float scaleMultiplier)
    {
        if (target == null) yield break;
        Vector3 originalScale = Vector3.one;
        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // Scale Up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            // Smooth step ease
            t = t * t * (3f - 2f * t);
            target.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, t);
            yield return null;
        }

        // Scale Down
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            // Smooth step ease
            t = t * t * (3f - 2f * t);
            target.localScale = Vector3.Lerp(originalScale * scaleMultiplier, originalScale, t);
            yield return null;
        }
        target.localScale = originalScale;
    }

    public static IEnumerator SlideIn(RectTransform target, Vector2 startAnchoredPos, Vector2 endAnchoredPos, float duration)
    {
        if (target == null) yield break;
        target.anchoredPosition = startAnchoredPos;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease Out Cubic
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            target.anchoredPosition = Vector2.Lerp(startAnchoredPos, endAnchoredPos, t);
            yield return null;
        }
        target.anchoredPosition = endAnchoredPos;
    }
    
    // Helper to start coroutine on a MonoBehaviour safely
    public static void Start(MonoBehaviour host, IEnumerator routine)
    {
        if (host != null && host.gameObject.activeInHierarchy)
        {
            host.StartCoroutine(routine);
        }
    }
}
