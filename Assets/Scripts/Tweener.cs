using UnityEngine;

namespace Assets.Scripts
{
    public class Tweener : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 endPosition;
        
        private bool isTweening;
        private float startTime;
        private float tweenDuration;

        public void TweenPosition(float duration, Vector3 position)
        {
            startTime = Time.time;
            tweenDuration = duration;
            startPosition = transform.localPosition;
            endPosition = position;
            isTweening = true;
        }

        private void Update()
        {
            if (isTweening)
            {
                if (Time.time - startTime > tweenDuration)
                {
                    isTweening = false;
                }

                transform.localPosition = Vector3.Lerp(startPosition, endPosition, Mathf.Clamp01((Time.time - startTime) / tweenDuration));
            }
        }
    }
}