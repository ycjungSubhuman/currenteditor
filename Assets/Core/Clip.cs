using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public class Clip
    {
        private AudioSource audio;
        private Midi.Midi midi;

        public AudioSource Audio { get { return audio; } }
        public Midi.Midi Midi { get { return midi; } }

        public Clip(AudioSource audio, Midi.Midi midi)
        {
            this.audio = audio;
            this.midi = midi;
        }

        public void Play()
        {
            audio.Play();
        }
    }
}
