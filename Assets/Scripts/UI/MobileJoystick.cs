using UnityEngine;
using UnityEngine.EventSystems;
using Lumber.Player;

namespace Lumber.UI
{
    /// On-screen virtual joystick. Works with both touch and mouse (Editor testing)
    /// since it's driven by Unity's pointer event system, not raw Input.touches.
    public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform background;
        public RectTransform handle;
        public float handleRange = 55f;
        public FirstPersonController target;

        private Vector2 inputVector;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            inputVector = Vector2.ClampMagnitude(localPoint / handleRange, 1f);
            if (handle != null)
                handle.anchoredPosition = inputVector * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }

        private void Update()
        {
            if (target != null)
                target.moveInput = inputVector;
        }
    }
}
