using UnityEngine;
using System.Collections;

namespace CommonGesture {
  public class MapCameraManager : MonoBehaviour {
    public float maxXPosition;
    public float maxYPosition;
    public float maxOrthographicSize;
    public float minOrthographicSize;

    private Camera    orthCamera;
    private Transform camTrans;

    public void Initialize(Gesture gesture, Camera camera) {
      gesture.OnSwipe         += (center, move, id) => MoveCamera(move);
      gesture.OnMomentumSwpie += (center, move, id) => MoveCamera(move);
      gesture.OnPinch         += ZoomCamera;

      this.orthCamera = camera;
      this.camTrans   = camera.transform;
    }

    public void SetMaxPositionFromSprite(Sprite sprite) {
      Bounds bounds = sprite.bounds;
      maxXPosition = bounds.extents.x;
      maxYPosition = bounds.extents.y;
    }

    public void MoveCamera(Vector2 scrDelta) {
      Vector2 move = scrDelta;

      // move position
      Vector2 uniMov = move * (orthCamera.orthographicSize / (Screen.height * 0.5f));
      Vector3 cpos   = camTrans.localPosition - new Vector3(uniMov.x, uniMov.y, 0f);

      // clamp
      float hMargin = orthCamera.orthographicSize;
      float wMargin = hMargin * (Screen.width / Screen.height);
      cpos.x = Mathf.Clamp(cpos.x, -(maxXPosition - wMargin), maxXPosition - wMargin);
      cpos.y = Mathf.Clamp(cpos.y, -(maxYPosition - hMargin), maxYPosition - hMargin);

      camTrans.localPosition = cpos;
    }

    public void ZoomCamera(Vector2 scrCenter, Vector2 scrMovePoint, float magnitude) {
      Vector2 center = scrCenter;
      Vector2 pos    = scrMovePoint;

      // centered-zoom
      float baseMagnitude = (pos - center).magnitude;
      float headMagnitude = (pos + (pos - center).normalized * magnitude - center).magnitude;
      float deltaScale = headMagnitude / baseMagnitude;
      float prevOrthSize = orthCamera.orthographicSize;
      float orthSize = Mathf.Clamp(prevOrthSize / deltaScale, minOrthographicSize, maxOrthographicSize);
      orthCamera.orthographicSize = orthSize;

      // center-transform
      Vector2 uniCenter = center - new Vector2(Screen.width, Screen.height) * 0.5f;
      Vector2 modCenter = uniCenter * (prevOrthSize / orthSize);
      Vector2 move = uniCenter - modCenter;
      MoveCamera(move);
    }

  }

}
