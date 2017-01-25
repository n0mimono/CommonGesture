using UnityEngine;
using System.Collections;

namespace CommonGesture {
  public class SmoothVector {
    private int length;
    private float range;
    private Vector2[] seq;
    private float[] delta;
    private int curIndex;
    private Vector2 curVec;

    public Vector2 CurrentVector {
      get {
        return curVec;
      }
    }

    public SmoothVector(int length, float range) {
      this.length = length;
      this.range = range;
      seq = new Vector2[length];
      delta = new float[length];
      curIndex = 0;
    }

    public void AddValue(Vector2 vec, float d) {
      seq[curIndex] = vec;
      delta[curIndex] = d;

      Vector2 wsum = Vector2.zero;
      float weights = 0f;
      for (int i = 0; i < length; i++) {
        int index = (curIndex - i + length) % length;
        float w = delta[index] * (1f / (i + 1));
        wsum += seq[index] * w;
        weights += w;
        if (weights >= range) break;
      }

      curIndex = (curIndex + 1) % length;
      curVec = wsum * (1f / weights);
    }

  }
}
