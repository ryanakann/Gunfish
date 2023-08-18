using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class MarqueeManager : PersistentSingleton<MarqueeManager> {
    private struct MarqueeContents {
        public string text;
        public float duration;
        public AnimationCurve tween;
        public Action callback;

        public MarqueeContents(string text, float duration, AnimationCurve tween, Action callback) {
            this.text = text;
            this.duration = duration;
            this.tween = tween;
            this.callback = callback;
        }
    }

    [SerializeField]
    private float defaultDuration = 1f;
    [SerializeField]
    private AnimationCurve defaultTween = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private string[] quips;

    private TMP_Text textAsset;
    private Queue<MarqueeContents> queue = new();

    private bool transitioning = false;

    private void Start() {
        textAsset = GetComponentInChildren<TMP_Text>();
        textAsset.SetText("");
    }

    private void Update() {
        UpdateDebug();
    }

    private void UpdateDebug() {
        if (!GameManager.debug) return;

        if (Input.GetKeyDown(KeyCode.P)) {
            Enqueue("3");
            Enqueue("2");
            Enqueue("1");
            Enqueue("GO", () => { Debug.Log("Countdown over!"); });
        }
    }

    public void EnqueueRandomQuip() {
        if (quips == null || quips.Length == 0) {
            Debug.LogWarning("Could not enqueue quip. Make sure you have at least one in the MarqueeManager");
            return;
        }

        var index = UnityEngine.Random.Range(0, quips.Length);
        var quip = quips[index];
        Enqueue(quip);
    }

    public void Enqueue(string text, Action callback = null) {
        Enqueue(text, defaultDuration, defaultTween, callback);
    }

    public void Enqueue(string text, float duration, Action callback = null) {
        Enqueue(text, duration, defaultTween, callback);
    }

    public void Enqueue(string text, float duration, AnimationCurve tween, Action callback = null) {
        var contents = new MarqueeContents(text, duration, tween, callback);
        queue.Enqueue(contents);
        if (!transitioning) {
            transitioning = true;
            StartCoroutine(Scroll());
        }
    }

    private void OnGUI() {
        if (!GameManager.debug) return;
        GUI.TextField(new Rect(0f, 20f, 200f, 20f), "Press P to start a countdown");
    }

    private IEnumerator Scroll() {
        while (queue.Count > 0) {
            MarqueeContents contents;
            if (!queue.TryDequeue(out contents)) {
                continue;
            }
            
            textAsset.SetText(contents.text);
            
            float t = 0f;
            while (t < 1f) {
                var tween = contents.tween.Evaluate(t);
                var value = Mathf.Lerp(Screen.width, -Screen.width, tween);

                textAsset.rectTransform.anchoredPosition = new Vector2(value, 0f);

                t += Time.deltaTime / contents.duration;
                yield return new WaitForEndOfFrame();
            }

            contents.callback?.Invoke();
        }
        textAsset.SetText("");
        transitioning = false;
    }
}