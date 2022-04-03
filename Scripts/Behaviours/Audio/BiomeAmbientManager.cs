using System.Collections;
using Procedural.Biomes;
using Procedural.Terrain;
using UnityEngine;

namespace Procedural.Audio
{
    public class BiomeAmbientManager : MonoBehaviour
    {
        // TODO: time of day
        // TODO: weather
        // Idea: clear sky = birds | rain = rain sound | always wind forest sound

        [SerializeField]
        private Transform _player;
        [SerializeField]
        private MapGenerator _mapGenerator;
        [SerializeField]
        private AudioSource _audioPrevious;
        [SerializeField]
        private AudioSource _audioCurrent;
        [SerializeField]
        private float _transitionTime = 3;
        [SerializeField]
        private float _transitionCooldown = 2;
        
        private float _targetSoundLevel = 1;
        private BiomeDeterminer _biomeDeterminer;
        private TerrainType _currentBiome;
        private bool _transitionActive = false;

        private void Start()
        {
            _biomeDeterminer = _mapGenerator.BiomeDeterminer;
            _targetSoundLevel = _audioCurrent.volume;
        }

        private void Update()
        {
            Vector3 playerPos = _player.position;
            Vector2 playerPosV2 = new Vector2(playerPos.x, playerPos.z);

            TerrainType newBiome = _biomeDeterminer.GetBiomeAt(playerPosV2);

            if(newBiome != _currentBiome && TryTransitionAudio(newBiome.AmbientTrack))
            {
                // biome changed
                _currentBiome = newBiome;
            }
        }

        private bool TryTransitionAudio(AudioClip newAudio)
        {
            if(_transitionActive)
                return false;

            StartCoroutine(AudioTransition(_currentBiome.AmbientTrack, newAudio, _transitionTime));
            return true;
        }

        private IEnumerator AudioTransition(AudioClip from, AudioClip to, float time)
        {
            _transitionActive = true;
            float t = 0;

            _audioPrevious.clip = from;
            _audioPrevious.Play();
            _audioCurrent.clip = to;
            _audioCurrent.Play();

            while(t < time)
            {
                float alphaTo = t / time;
                float alphaFrom = 1 - alphaTo;

                _audioPrevious.volume = alphaFrom * _targetSoundLevel;
                _audioCurrent.volume = alphaTo * _targetSoundLevel;

                t += Time.deltaTime;
                yield return null;
            }
            
            _audioPrevious.volume = 0;
            _audioCurrent.volume = _targetSoundLevel;

            yield return new WaitForSeconds(_transitionCooldown);
            _transitionActive = false;
        }
    }
}