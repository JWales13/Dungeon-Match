using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Concrete IPointerInputSource backed by Unity's legacy Input class.
    /// On touch devices, Unity reports the primary touch through Input.mousePosition
    /// / GetMouseButtonDown as well, so this works for both editor testing (mouse)
    /// and on-device play (touch) without extra branching.
    /// </summary>
    public class UnityPointerInputSource : MonoBehaviour, IPointerInputSource
    {
        public event Action<Vector2> PointerDown;
        public event Action<Vector2> PointerUp;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerDown?.Invoke(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                PointerUp?.Invoke(Input.mousePosition);
            }
        }
    }
}