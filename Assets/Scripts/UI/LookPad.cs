using UnityEngine;
using UnityEngine.EventSystems;
using Lumber.Player;

namespace Lumber.UI
{
    /// Large invisible drag zone covering the right side of the screen for camera look.
    public class LookPad : MonoBehaviour, IDragHandler
    {
        public FirstPersonController target;
        public float touchSensitivity = 0.25f;

        public void OnDrag(PointerEventData eventData)
        {
            if (target == null) return;
            target.lookInput += new Vector2(eventData.delta.x, eventData.delta.y) * touchSensitivity;
        }
    }
}
