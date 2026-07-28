using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    /// <summary>
    /// The Green Room (hub) scene's controller. For now it just launches a
    /// floor via the Play button. Currencies, the Bomb Bench, and upgrades move
    /// here in the following micro-steps.
    /// </summary>
    public class GreenRoomController : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        private void Awake()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(Play);
            }
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(Play);
            }
        }

        private void Play()
        {
            SceneManager.LoadScene(SceneNames.Floor);
        }
    }
}