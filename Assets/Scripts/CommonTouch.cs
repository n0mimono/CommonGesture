using UnityEngine;
using System.Collections;

namespace CommonGesture {

  public struct CommonTouch {
    public bool       active;
    public int        index;
    public int        id;
    public Vector2    pos;
    public Vector2    delta;
    public float      mag;
    public float      time;
    public TouchPhase phase;

    public static int MouseTouchId { get { return 200; } }

    public void Clear() {
      active = false;
      id     = -1;
    }

    private static Vector2 prevMousePosition;

    public static CommonTouch CreateFromTouch(int i) {
      Touch touch = Input.GetTouch (i);
      CommonTouch ct = new CommonTouch () {
        active = true,
        index  = i,
        id     = touch.fingerId,
        pos    = touch.position,
        delta  = touch.deltaPosition,
        mag    = touch.deltaPosition.magnitude,
        time   = Time.time,
        phase  = touch.phase
      };
      return ct;
    }

    public static CommonTouch CreateFromMouse() {
      // we believe that this function is called on each frames.

      CommonTouch ct = new CommonTouch ();
      ct.phase = TouchPhase.Canceled;

      if (Input.touchCount == 0) {
        if (Input.GetMouseButtonDown (0)) {
          ct.phase = TouchPhase.Began;
        } else if (Input.GetMouseButtonUp (0)) {
          ct.phase = TouchPhase.Ended;
        } else if (Input.GetMouseButton (0)) {
          ct.phase = TouchPhase.Moved;
        }
      }

      if (ct.phase != TouchPhase.Canceled) {
        ct.active = true;
        ct.index  = 0;
        ct.id     = MouseTouchId;
        ct.pos    = Input.mousePosition;
        ct.delta = Vector2.zero;
        if (ct.phase != TouchPhase.Began) {
          ct.delta = ct.pos - prevMousePosition;
        }
        ct.mag    = ct.delta.magnitude;
        ct.time   = Time.time;
      }

      prevMousePosition = ct.pos;
      return ct;
    }

  }

}
