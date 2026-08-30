using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    public class SettingForm : UGuiForm
    {
        [SerializeField]
        private Toggle m_MusicMuteToggle = null;

        [SerializeField]
        private Slider m_MusicVolumeSlider = null;

        [SerializeField]
        private Toggle m_SoundMuteToggle = null;

        [SerializeField]
        private Slider m_SoundVolumeSlider = null;

        // [SerializeField]
        // private Toggle m_UISoundMuteToggle = null;

        // [SerializeField]
        // private Slider m_UISoundVolumeSlider = null;


        public void OnMusicMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("Music", !isOn);
            m_MusicVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnMusicVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Music", volume);
        }

        public void OnSoundMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("Sound", !isOn);
            m_SoundVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnSoundVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Sound", volume);
        }

        // public void OnUISoundMuteChanged(bool isOn)
        // {
        //     GameEntry.Sound.Mute("UISound", !isOn);
        //     m_UISoundVolumeSlider.gameObject.SetActive(isOn);
        // }

        // public void OnUISoundVolumeChanged(float volume)
        // {
        //     GameEntry.Sound.SetVolume("UISound", volume);
        // }



#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_MusicMuteToggle.isOn = !GameEntry.Sound.IsMuted("Music");
            m_MusicVolumeSlider.value = GameEntry.Sound.GetVolume("Music");

            m_SoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("Sound");
            m_SoundVolumeSlider.value = GameEntry.Sound.GetVolume("Sound");

            // m_UISoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("UISound");
            // m_UISoundVolumeSlider.value = GameEntry.Sound.GetVolume("UISound");


        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);


        }
    }
}
