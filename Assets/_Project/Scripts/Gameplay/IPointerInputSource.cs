using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Abstracts "where did the player press/release the screen" away from any
    /// specific Unity input API. InputController depends on this interface,
    /// not on Input.* directly, so the underlying input method (legacy Input,
    /// the new Input System, mouse vs. touch) can change without touching
    /// swipe/gesture logic.
    /// </summary>
    public interface IPointerInputSource
    {
        event Action<Vector2> PointerDown;
        event Action<Vector2> PointerUp;
    }
}