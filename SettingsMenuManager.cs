using System.Collections.Generic; // Add this namespace

namespace MyGame.Settings
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Audio;
    using TMPro;
    using NUnit.Framework;

    public class SettingsMenuManager : MonoBehaviour
    {
        public AudioMixer mainAudioMixer;
        public Slider MasterVolume, SFXVolume;
        //public Dropdown resolutionDropdown;
        public TMP_Dropdown resolutionDropdown;

        Resolution[] resolutions;

        private void Start()
        {
            resolutions = Screen.resolutions;

            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        public void ChangeMasterVolume()
        {
            mainAudioMixer.SetFloat("MasterVolume", MasterVolume.value);
        }

        public void ChangeSfxVolume()
        {
            mainAudioMixer.SetFloat("SFXVolume", SFXVolume.value);
        }
    }
}
