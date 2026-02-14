using UnityEngine;

namespace OneStrokeRGR.Sound
{
    /// <summary>
    /// サウンド管理クラス（シングルトン）
    /// InspectorでBGM/SEのAudioClipを設定する
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip normalBGM;
        public AudioClip bossBGM;

        [Range(0f, 1f)]
        public float bgmVolume = 0.5f;

        [Header("SE")]
        public AudioClip moveSE;
        public AudioClip attackSE;
        public AudioClip getPowerSE;
        public AudioClip getGoldSE;
        public AudioClip getHealSE;
        public AudioClip enemyActionSE;
        public AudioClip takeDamage;
        public AudioClip pushButton;

        [Range(0f, 1f)]
        public float seVolume = 1f;

        [Header("SE同時再生数")]
        public int seSourceCount = 5;

        private AudioSource bgmSource;
        private AudioSource[] seSources;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;

            seSources = new AudioSource[seSourceCount];
            for (int i = 0; i < seSourceCount; i++)
            {
                seSources[i] = gameObject.AddComponent<AudioSource>();
                seSources[i].loop = false;
                seSources[i].playOnAwake = false;
            }
        }

        // ========== BGM ==========

        public void PlayNormalBGM()
        {
            PlayBGM(normalBGM);
        }

        public void PlayBossBGM()
        {
            PlayBGM(bossBGM);
        }

        private void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

            if (bgmSource.isPlaying && bgmSource.clip == clip)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmSource.volume = bgmVolume;
        }

        // ========== SE ==========

        public void PlayMoveSE()
        {
            PlaySE(moveSE);
        }

        public void PlayAttackSE()
        {
            PlaySE(attackSE);
        }

        public void PlayGetPowerSE()
        {
            PlaySE(getPowerSE);
        }

        public void PlayGetGoldSE()
        {
            PlaySE(getGoldSE);
        }

        public void PlayGetHealSE()
        {
            PlaySE(getHealSE);
        }
        public void PlayEnemyActionSE()
        {
            PlaySE(enemyActionSE);
        }
        private void PlaySE(AudioClip clip)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSESource();
            if (source == null) return;

            source.clip = clip;
            source.volume = seVolume;
            source.Play();
        }

        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
        }

        public void StopAllSE()
        {
            foreach (var source in seSources)
            {
                source.Stop();
                source.clip = null;
            }
        }

        private AudioSource GetAvailableSESource()
        {
            foreach (var source in seSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return null;
        }
    }
}
