using System;
using UnityEngine;

// Spelfysik inlämningsuppgift 2B
namespace Spelfysik2B
{
    public class CameraControl : MonoBehaviour
    {
        private Camera camera;
        public float scale = 1f;
        public float dragFactor = 0.5f;
        private Vector3 dragOrigin;
        private bool Enabled = false;

        public void GUIToggle(Boolean gui_value)
        {
            Enabled = gui_value;
        }

        private void Start()
        {
            camera = gameObject.GetComponent<Camera>();
        }

        private void Update()
        {
            if (Enabled)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                {
                    transform.position += Vector3.up * Input.mouseScrollDelta.y * scale;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                {
                    transform.position += Vector3.up * Input.mouseScrollDelta.y * scale;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    dragOrigin = Input.mousePosition;
                    return;
                }
                else if (!Input.GetMouseButton(0))
                {
                    return;
                }

                Vector3 pos = camera.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = new Vector3(pos.x * dragFactor, 0, pos.y * dragFactor);

                transform.Translate(move, Space.World);
            }
        }

    }
}
