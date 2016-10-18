# CommonGesture

Simple touch gesture sample.

---

# Open API

## Parameters 

```c#
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
      public int   smoothArrayLength  = 5;
      public float smoothDeltaRange   = 0.15f;
    }
    public SwipeOptions swipeOpt;

    [Serializable]
    public class PinchOptions {
      public float detectMagnitude = 0.1f;
    }
    public PinchOptions pinchOpt;
```

## Delegates

```c#
    public event SwipeHandler OnSwipe;
    public event SwipeHandler OnMomentumSwpie;
    public event PinchHandler OnPinch;
    public event TouchHandler OnTouchDown;
    public event TouchHandler OnTouching;
    public event TouchHandler OnTouchUp;
    public event TouchHandler OnTap;
    public event TouchHandler OnDoubleTap;
    public event TouchHandler OnLongTouch;
    public TouchAreaHandler IsTouchArea = ((pos, id) => true);
```

---

# Helper components

## MapCameraManager

Bridge component sample btw gesture and camera.

## GestureAreaManager

Sample of gesture area handling.
