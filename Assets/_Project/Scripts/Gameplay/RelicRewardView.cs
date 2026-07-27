using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// The post-room "pick 1 of 2 relics" panel. Shows each option's name and
    /// description on a button and raises RelicChosen(index) when tapped. It
    /// only presents options and reports the choice; GameController owns what
    /// happens to the run.
    /// </summary>
    public class RelicRewardView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        [Header("Option A")]
        [SerializeField] private Button _optionAButton;
        [SerializeField] private TMP_Text _optionATitle;
        [SerializeField] private TMP_Text _optionADescription;

        [Header("Option B")]
        [SerializeField] private Button _optionBButton;
        [SerializeField] private TMP_Text _optionBTitle;
        [SerializeField] private TMP_Text _optionBDescription;

        public event Action<int> RelicChosen;

        private void Awake()
        {
            _optionAButton.onClick.AddListener(ChooseA);
            _optionBButton.onClick.AddListener(ChooseB);
            Hide();
        }

        private void OnDestroy()
        {
            _optionAButton.onClick.RemoveListener(ChooseA);
            _optionBButton.onClick.RemoveListener(ChooseB);
        }

        public void Show(IReadOnlyList<IRelic> options)
        {
            PopulateOption(options, 0, _optionAButton, _optionATitle, _optionADescription);
            PopulateOption(options, 1, _optionBButton, _optionBTitle, _optionBDescription);
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private static void PopulateOption(IReadOnlyList<IRelic> options, int index, Button button, TMP_Text title, TMP_Text description)
        {
            bool hasOption = index < options.Count;
            button.gameObject.SetActive(hasOption);

            if (!hasOption)
            {
                return;
            }

            title.text = options[index].DisplayName;
            description.text = options[index].Description;
        }

        private void ChooseA() => RelicChosen?.Invoke(0);

        private void ChooseB() => RelicChosen?.Invoke(1);
    }
}