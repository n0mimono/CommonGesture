using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GestureAreaManager : MonoBehaviour {
  public GameObject targetPanel;

  public Bounds[] bounds;

  void Start() {
    bounds = targetPanel
      .GetComponentsInChildren<Image> (false)
      .Where(i => i.enabled)
      .Where (i => i.raycastTarget)
      .Select (i => i.GetComponent<RectTransform> ())
      .Select (t => {
        Vector2 size  = t.sizeDelta;
        Vector3 pos   = t.position;
        Vector2 pivot = t.pivot - 0.5f * Vector2.one;
        return new Bounds (new Vector2(pos.x, pos.y) - new Vector2(pivot.x * size.x, pivot.y * size.y), size);
      }).ToArray ();

  }

  public bool IsInTouchArea(Vector2 srcPos) {
    for (int i = 0; i < bounds.Length; i++) {
      if (bounds [i].Contains (srcPos)) return false;
    }
    return true;
  }

}
