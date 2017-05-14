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
        private IEnumerable<Midi.MidiMessage> originalMidiMessages = new List<Midi.MidiMessage>();
        private double currTime;
        public double CurrTime { get { return currTime; } }

        public AudioSource Audio { get { return audio; } }
        public Midi.Midi Midi { get { return midi; } }

        public Clip(AudioSource audio, Midi.Midi midi)
        {
            this.audio = audio;
            this.midi = midi;
            this.originalMidiMessages =
                from trk in midi.Tracks
                from msg in trk.Messages
                select msg;
            this.midiMessages = originalMidiMessages.Select(m => m);
            nonActive = midiMessages.Select(m => m).ToList();
        }

        public void Play()
        {
            currTime = 0;
            audio.Play();
        }

        List<Midi.MidiMessage> nonActive;
        public void UpdateTime()
        {
            if (nonActive.Count() == 0)
                midiMessages = originalMidiMessages.Select(m => m);
            else
                midiMessages = nonActive.Select(m => m);
            currTime = audio.time;
            nonActive = new List<Assets.Midi.MidiMessage>();
            foreach (var msg in midiMessages)
            {
                if (Math.Abs(msg.Time - currTime) >= delta && msg.Time-delta > currTime)
                {
                    nonActive.Add(msg);
                }
            }
            currTime = audio.time;
        }

        private const double delta = 0.02; //30ms
        public IEnumerable<Midi.MidiMessage> GetCurrMessages(int channel, Midi.MidiMessage.Type type)
        {
            List<Midi.MidiMessage> targets = new List<Assets.Midi.MidiMessage>();
            foreach (var msg in midiMessages)
            {
                if(Math.Abs(msg.Time - currTime) < delta && audio.isPlaying && msg.Channel == channel && msg.MessageType == type)
                {
                    targets.Add(msg);
                }
            }
            return targets;
        }

    }
}
