using System;
using System.Collections;
using UnityEngine;
using Lumber.World;

namespace Lumber.Player
{
    /// Handles the axe swing animation and the forward raycast that damages trees.
    public class AxeTool : MonoBehaviour
    {
        public Transform swingPivot;
        public float range = 3f;
        public float swingDuration = 0.28f;
        public LayerMask treeLayer = ~0;

        public int damage = 10;
        public float cooldown = 0.45f;

        public event Action<Tree, int> OnTreeHit;

        private bool swinging;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        public void SetStats(int newDamage, float newCooldown)
        {
            damage = newDamage;
            cooldown = newCooldown;
        }

        public void TryChop()
        {
            if (swinging) return;
            StartCoroutine(SwingRoutine());
        }

        private IEnumerator SwingRoutine()
        {
            swinging = true;

            Quaternion startRot = swingPivot != null ? swingPivot.localRotation : Quaternion.identity;
            Quaternion swingRot = startRot * Quaternion.Euler(-70f, 0f, 15f);

            float half = swingDuration * 0.5f;
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                if (swingPivot != null)
                    swingPivot.localRotation = Quaternion.Slerp(startRot, swingRot, t / half);
                yield return null;
            }

            ApplyHit();

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                if (swingPivot != null)
                    swingPivot.localRotation = Quaternion.Slerp(swingRot, startRot, t / half);
                yield return null;
            }
            if (swingPivot != null) swingPivot.localRotation = startRot;

            float remaining = cooldown - swingDuration;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            swinging = false;
        }

        private void ApplyHit()
        {
            if (cam == null) return;

            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, treeLayer, QueryTriggerInteraction.Collide))
            {
                var tree = hit.collider.GetComponentInParent<Tree>();
                if (tree != null && tree.IsChoppable)
                {
                    tree.TakeHit(damage);
                    OnTreeHit?.Invoke(tree, damage);
                }
            }
        }
    }
}
