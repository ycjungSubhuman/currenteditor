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
        private IEnumerable<Midi.MidiMessage> midiMessages = new List<Midi.MidiMessage>();
        private double currTime;
        public double CurrTime { get { return currTime; } }

        public AudioSource Audio { get { return audio; } }
        public Midi.Midi Midi { get { return midi; } }

        public Clip(AudioSource audio, Midi.Midi midi)
        {
            this.audio = audio;
            this.midi = midi;
            this.midiMessages =
                from trk in midi.Tracks
                from msg in trk.Messages
                select msg;
        }

        public void Play()
        {
            currTime = 0;
            audio.Play();
        }

        public void UpdateTime()
        {
            currTime = audio.time;
        }

        private const double delta = 0.03; //30ms
        public IEnumerable<Midi.MidiMessage> GetCurrMessages(int channel, Midi.MidiMessage.Type type)
        {
            var targets = from msg in midiMessages
                          where Math.Abs(msg.Time - currTime) < delta
                          select msg;
            return targets;
        }

    }
}
