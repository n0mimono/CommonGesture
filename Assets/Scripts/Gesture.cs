using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CommonGesture {

  public partial class Gesture : MonoBehaviour {

    // Component paramaters
    [Serializable]
    public class TouchOptions {
      public float tapDist       = 100f;
      public float tapTime       = 0.4f;
      public float longTouchTime = 1f;
      public float longTouchDist = 100f;
    }
    public TouchOptions touchOpt;

    [Serializable]
    public class SwipeOptions {
      public float detectDist = 0.1f;

      public float detectMomentumDist = 0.2f;
      public float momentumDamp       = 0.8f;
    }
    public SwipeOptions swipeOpt;

    [Serializable]
    public class PinchOptions {
      public float detectMagnitude = 0.1f;
    }
    public PinchOptions pinchOpt;

    // event handlers
    public delegate void SwipeHandler(Vector2 center, Vector2 move, int id);
    public delegate void PinchHandler(Vector2 center, Vector2 pos, float magnitude);
    public delegate void TouchHandler(Vector2 pos, int id);
    public delegate bool TouchAreaHandler(Vector2 pos, int id);

    // open delegates
    public SwipeHandler     OnSwipe         = ((center, move, id) => {});
    public SwipeHandler     OnMomentumSwpie = ((center, move, id) => {});
    public PinchHandler     OnPinch         = ((center, pos, magnitude) => {});
    public TouchHandler     OnTouchDown     = ((pos, id) => {});
    public TouchHandler     OnTouching      = ((pos, id) => {});
    public TouchHandler     OnTouchUp       = ((pos, id) => {});
    public TouchHandler     OnTap           = ((pos, id) => {});
    public TouchHandler     OnDoubleTap     = ((pos, id) => {});
    public TouchHandler     OnLongTouch     = ((pos, id) => {});
    public TouchAreaHandler IsTouchArea     = ((pos, id) => true);

    // dev delegates
    public TouchHandler     OnTouchCenter   = ((pos, id) => {});

    public void SetActive(bool isActive) {
      cur.isActive = isActive;
    }

    public bool HasTouch() {
      return touchCount > 0;
    }
  }

  public partial class Gesture {

    // current state
    private class State {
      public bool    isActive;

      public int     primaryIndex;
      public Vector2 center;
      public Vector2 delta;
      public Vector2 momentumMov;

      public void Clear() {
        primaryIndex  = 0;
        center        = Vector2.zero;
        delta         = Vector2.zero;
      }
    }
    private State cur = new State();

    // wrapped touch array
    private const int MaxTouchCount = 6;
    private CommonTouch[] touches = new CommonTouch[MaxTouchCount];
    private int           touchCount;

    private Dictionary<int,CommonTouch> touchDownMap = new Dictionary<int,CommonTouch>(MaxTouchCount);
    private Dictionary<int,CommonTouch> touchStopMap = new Dictionary<int,CommonTouch>(MaxTouchCount);

    void Start() {
      SetActive (true); // temp
    }

    void Update() {
      if (!cur.isActive) return;

      UpdateCollectTouches ();
      UpdatePreProcess ();
      UpdateEarlyProcess ();
      UpdateLateProcess ();
      UpdatePostProcess ();
    }

    private void UpdateCollectTouches() {
      touchCount = Input.touchCount;
      for (int i = 0; i < touchCount; i++) {
        touches [i] = CommonTouch.CreateFromTouch (i);
      }

      #if UNITY_EDITOR
      CommonTouch mouseTouch = CommonTouch.CreateFromMouse();
      if (mouseTouch.active) {
        touchCount = 1;
        touches[0] = mouseTouch;
      }
      #endif
    }

    private void UpdatePreProcess() {
      cur.Clear ();

      if (touchCount > 0) {
        RecalculateTouchCenter ();
      }
    }

    private void UpdateEarlyProcess() {
      if (touchCount == 0) return;

      bool isPinchInvoked = InvokePinchEvent();
      if (!isPinchInvoked || true) { // test
        InvokeSwipeEvent ();
      }
    }

    private void UpdateLateProcess() {

      for (int i = 0; i < touchCount; i++) {
        CommonTouch touch = touches [i];
        if (touch.phase == TouchPhase.Began) {
          BeginTouchEvent(touch);
        } else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
          TouchingEvent(touch);
        } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
          EndTouchEvent(touch);
        }
      }

    }

    private void UpdatePostProcess() {
      InvokeMomentumSwipe ();

      // other events
      OnTouchCenter.Invoke (cur.center, cur.primaryIndex);
    }

    private void BeginTouchEvent(CommonTouch touch) {
      if (!IsTouchArea(touch.pos, touch.id)) return;

      RegisterTouchDown (touch);
      RegisterTouchStop (touch);
      CancelmomentumSlide ();

      OnTouchDown.Invoke(touch.pos, touch.id);
    }

    private void TouchingEvent(CommonTouch touch) {
      RecalculateTouchStop (touch);
      InvokeLongTouchEvent (touch);

      OnTouching.Invoke (touch.pos, touch.id);
    }

    private void EndTouchEvent(CommonTouch touch) {
      InvokeTapEvent (touch);
      RegisterMomentumSwipe ();

      OnTouchUp.Invoke(touch.pos, touch.id);
    }

    private void RecalculateTouchCenter() {
      // calculate weighted touch center
      float   weight = 0f;
      Vector2 center = Vector2.zero;
      Vector2 delta  = Vector2.zero;

      float maxMag = 0f;
      for (int i = 0; i < touchCount; i++) {
        CommonTouch touch = touches [i];

        float mag   = touch.mag;
        float w     = Mathf.Max(1f, 1f / (mag + 1e-5f));
        weight += w;
        center += touch.pos * w;
        delta  += touch.delta * w;

        // update primary touch and its magnitude
        if (maxMag < mag) {
          maxMag = mag;
          cur.primaryIndex = i;
        }
      }

      // update touch center
      cur.center = center * (1f / weight);
      cur.delta  = delta  * (1f / weight);
    }

    private bool InvokePinchEvent() {
      if (touchCount < 2) return false;

      CommonTouch touch = touches [cur.primaryIndex];
      Vector2 pos   = (touch.pos - cur.center).normalized;
      Vector2 vec   = touch.delta;
      float   mag   = Vector2.Dot(pos, vec);

      bool isPinchInvoked = Mathf.Abs(mag) >= pinchOpt.detectMagnitude;
      if (isPinchInvoked) {
        OnPinch.Invoke (cur.center, touch.pos, mag);
      }

      return isPinchInvoked;
    }

    private bool InvokeSwipeEvent() {
      if (touchCount < 1) return false;

      Vector2 mov = cur.delta;
      float   mag = cur.delta.magnitude;

      bool isSwipeInvoked = mag >= swipeOpt.detectDist;
      if (isSwipeInvoked) {
        OnSwipe.Invoke (cur.center, mov, cur.primaryIndex);
      }

      return isSwipeInvoked;
    }

    private void RegisterTouchDown(CommonTouch touch) {
      touchDownMap[touch.id] = touch;
    }

    private void InvokeTapEvent(CommonTouch touch) {
      if (touchDownMap.ContainsKey (touch.id)) {
        CommonTouch touchOnDown = touchDownMap [touch.id];
        float movMag = (touchOnDown.pos - touch.pos).magnitude;
        if (movMag < touchOpt.tapDist && touch.time < touchOnDown.time + touchOpt.tapTime) {
          OnTap.Invoke (touch.pos, touch.id);

          touchOnDown.Clear ();
          touchDownMap [touch.id] = touchOnDown;
        }
      }
    }

    private void RegisterTouchStop(CommonTouch touch) {
      touchStopMap [touch.id] = touch;
    }

    private void RecalculateTouchStop(CommonTouch touch) {
      if (touchStopMap.ContainsKey (touch.id)) {
        CommonTouch touchOnStop = touchStopMap [touch.id];
        if (!touchOnStop.active) return;

        float movMag = (touchOnStop.pos - touch.pos).magnitude;
        if (movMag > touchOpt.longTouchDist) {
          touchStopMap [touch.id] = touch;
        }
      }
    }

    private void InvokeLongTouchEvent(CommonTouch touch) {
      if (touchStopMap.ContainsKey (touch.id)) {
        CommonTouch touchOnStop = touchStopMap [touch.id];
        if (!touchOnStop.active) return;

        if (touch.time > touchOnStop.time + touchOpt.longTouchTime) {
          OnLongTouch (touch.pos, touch.id);

          touchOnStop.Clear ();
          touchStopMap [touch.id] = touchOnStop;
        }
      }
    }

    private void CancelmomentumSlide() {
      cur.momentumMov      = Vector2.zero;
    }

    private void RegisterMomentumSwipe() {
      if (touchCount == 1) {
        cur.momentumMov = cur.delta;
      }
    }

    private void InvokeMomentumSwipe() {
      if (cur.momentumMov.magnitude > swipeOpt.detectMomentumDist) {
        cur.momentumMov *= swipeOpt.momentumDamp;

        OnMomentumSwpie.Invoke (Vector2.zero, cur.momentumMov, 0);
      }
    }

  }

}
