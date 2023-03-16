using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.C_.GUI_scripts
{
    public class SliderTextUpdate : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Text _sliderText;

        private void Start()
        {
            // add a function listenere to update the text when the slider changes
            _slider.onValueChanged.AddListener((v) =>
           {
               _sliderText.text = v.ToString("0.000");
           }
            );
        }
    }
}
