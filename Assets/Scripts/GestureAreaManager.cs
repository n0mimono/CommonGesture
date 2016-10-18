using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GestureAreaManager : MonoBehaviour {
  public GameObject targetPanel;
  public RectTransform[] rects;

  void Start() {
    rects = targetPanel
      .GetComponentsInChildren<Image> (false)
      .Where (i => i.enabled)
      .Where (i => i.raycastTarget)
      .Select (i => i.GetComponent<RectTransform> ())
      .ToArray ();
  }

  public bool IsInTouchArea(Vector2 scrPos) {
    for (int i = 0; i < rects.Length; i++) {
      if (RectTransformUtility.RectangleContainsScreenPoint(rects[i], scrPos)) return false;
    }
    return true;
  }

}
