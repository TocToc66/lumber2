using System;
using System.Collections;
using UnityEngine;

namespace Lumber.World
{
    /// A choppable tree: takes hits from the player's axe, topples over and grants
    /// wood (money) + XP when felled, then regrows after a delay.
    public class Tree : MonoBehaviour
    {
        public int maxHealth = 40;
        public int woodValue = 4;
        public int xpValue = 8;
        public float regrowTime = 25f;

        public event Action<Tree> OnFelled;
        public event Action<Tree> OnRegrown;

        private int health;
        private bool felled;
        private bool busy;

        private Vector3 originalScale;
        private Vector3 originalLocalPosition;
        private Quaternion originalRotation;
        private Renderer[] renderers;
        private Collider[] colliders;

        private void Awake()
        {
            health = maxHealth;
            originalScale = transform.localScale;
            originalLocalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public bool IsChoppable => !felled;

        public void TakeHit(int damage)
        {
            if (felled || busy) return;

            health -= damage;
            StartCoroutine(HitFlash());

            if (health <= 0)
                StartCoroutine(FellAndRegrow());
        }

        private IEnumerator HitFlash()
        {
            Vector3 bumped = originalScale * 1.06f;
            transform.localScale = bumped;
            yield return new WaitForSeconds(0.08f);
            if (!felled)
                transform.localScale = originalScale;
        }

        private IEnumerator FellAndRegrow()
        {
            busy = true;
            felled = true;
            OnFelled?.Invoke(this);

            float t = 0f;
            const float duration = 0.6f;
            Quaternion start = transform.localRotation;
            Quaternion end = start * Quaternion.Euler(88f, UnityEngine.Random.Range(0, 360), 0f);
            Vector3 startPos = originalLocalPosition;

            while (t < duration)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / duration);
                transform.localRotation = Quaternion.Slerp(start, end, f);
                transform.localPosition = startPos + Vector3.down * (f * 0.4f);
                yield return null;
            }

            SetVisible(false);
            yield return new WaitForSeconds(regrowTime);

            transform.localRotation = originalRotation;
            transform.localPosition = originalLocalPosition;
            transform.localScale = originalScale;
            health = maxHealth;
            felled = false;
            busy = false;
            SetVisible(true);
            OnRegrown?.Invoke(this);
        }

        private void SetVisible(bool visible)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;

            foreach (var c in colliders)
                if (c != null) c.enabled = visible;
        }
    }
}
