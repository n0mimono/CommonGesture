using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CommonGesture {

  public class Demo : MonoBehaviour {
    public Gesture            gesture;
    public MapCameraManager   mapCamera;
    public GestureAreaManager gestureArea;

    [Header("UI")]
    public Image circle;
    public Image centerDot;
    public Text  logText;

    [Header("Object")]
    public Transform swipeObj;
    public Camera    camera;

    [Header("Extra")]
    public Sprite    sprite;

    //private Queue<string> logQueue = new Queue<string> ();

    void Start() {
      mapCamera.SetMaxPositionFromSprite (sprite);
      mapCamera.Initialize (gesture, camera);

      Gesture.SwipeHandler onSwipe = ((center, move, id) => {
        Vector3 angs = swipeObj.eulerAngles;
        angs.y -= move.x * 0.2f;
        angs.x -= move.y * 0.2f;
        if (angs.x > 180f && angs.x < 200f) angs.x = 200f;
        if (angs.x < 180f && angs.x > 160f) angs.x = 160f;
        swipeObj.eulerAngles = angs;
      });
      gesture.OnSwipe         += onSwipe;
      gesture.OnMomentumSwpie += onSwipe;
      gesture.OnSwipe         += (center, move, id) => Log("Swipe: " + center + ", " + move, Color.green);

      gesture.OnPinch         += ((center, pos, magnitude) => {
        RectTransform trans = centerDot.GetComponent<RectTransform>();
        Vector2 size = trans.sizeDelta + Vector2.one * magnitude;
        size.x = Mathf.Clamp(size.x, 0f, 200f);
        size.y = Mathf.Clamp(size.y, 0f, 200f);
        trans.sizeDelta = size;
      });

      gesture.OnTouchDown     += ((pos, id) => {
        StartCoroutine(EffectCircle(Color.red, pos, 1f));
      });
      gesture.OnTouchDown     += (pos, id) => Log("Down: " + id + ", " + pos, Color.red);

      gesture.OnTouchUp       += ((pos, id) => {
        StartCoroutine(EffectCircle(Color.blue, pos, 1f));
      });
      gesture.OnTouchUp       += (pos, id) => Log("Up: " + id + ", " + pos, Color.blue);

      gesture.OnTouching      += ((pos, id) => {
        // todo: something
      });
      gesture.OnTap           += ((pos, id) => {
        StartCoroutine(EffectCircle(Color.magenta, pos, 2f));
      });
      gesture.OnDoubleTap     += ((pos, id) => {
        // todo: code function
      });
      gesture.OnLongTouch     += ((pos, id) => {
        StartCoroutine(EffectCircle(Color.yellow, pos, 3f));
      });
      gesture.IsTouchArea      = ((pos, id) => {
        return gestureArea.IsInTouchArea(pos);
      });
      gesture.OnTouchCenter   += ((pos, id) => {
        centerDot.gameObject.SetActive(gesture.HasTouch());
        centerDot.GetComponent<RectTransform>().position = new Vector3(pos.x, pos.y, 0f);
      });

    }

    private IEnumerator EffectCircle(Color color, Vector2 pos, float scale) {
      Image image = Instantiate (circle);
      image.gameObject.SetActive (true);
      image.transform.SetParent (circle.transform.parent);

      RectTransform trans = image.GetComponent<RectTransform> ();
      Vector2 size = trans.sizeDelta * scale;

      //trans.anchoredPosition = pos;
      trans.position = new Vector3(pos.x, pos.y, 0f);
      for (float t = 0f; t < 1f; t += Time.deltaTime * 2f) {
        trans.sizeDelta = size * t;
        image.color = new Color (color.r, color.b, color.b, (1f - t));
        yield return null;
      }

      yield return null;
      Destroy (image.gameObject);
    }

    private void Log(string text, Color color) {
      //logQueue.Enqueue (string.Format ("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text));
      //if (logQueue.Count >= 30) logQueue.Dequeue ();
      //logText.text = logQueue.Aggregate ((a, s) => a += "\n" + s);
    }
  }

}
