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
        public bool updated = false;
        public double CurrTime { get { return currTime; } }
        public double ExpectedTime
        {
            get
            {
                return audio.clip.length - 4*Time.deltaTime;
            }
        }

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
        }

        public void Play()
        {
            currTime = 0;
            audio.time = 0;
            audio.volume = 1.0f;
        }
        public void Stop()
        {
            audio.volume = 0.0f;
            loops = 0;
        }
        public ClipTimeTracker GetTracker()
        {
            return new ClipTimeTracker(audio, originalMidiMessages.ToList());
        }
        int loops = 0;
        public void UpdateTime()
        {
            if (audio.clip.length * loops + audio.time < currTime) loops++;
            currTime = loops*audio.clip.length + audio.time;
            updated = true;
        }

        public class ClipTimeTracker
        {
            public IEnumerable<Midi.MidiMessage> midiMessages;
            private IEnumerable<Midi.MidiMessage> originalMidiMessages;
            private AudioSource audio;

            public ClipTimeTracker(AudioSource audio, List<Midi.MidiMessage> messages)
            {
                this.audio = audio;
                this.midiMessages = messages.Where(m => Math.Abs(m.Time - audio.time) >= delta).Select(m => m);
                this.originalMidiMessages = messages.Select(m => m);
            }

            const float delta = 0.02f;
            float lastTime = 0;
            public IEnumerable<Midi.MidiMessage> GetCurrMessages(int channel, Midi.MidiMessage.Type type)
            {
                float currTime = 0f;
                if (audio.volume != 0.0f) currTime = audio.time;
                else return new List<Midi.MidiMessage>();
                if(lastTime>currTime)
                {
                    Refill();
                }
                lastTime = currTime;
                List<Midi.MidiMessage> targets = new List<Assets.Midi.MidiMessage>();
                List<Midi.MidiMessage> afterConsume = new List<Assets.Midi.MidiMessage>();
                foreach (var msg in midiMessages)
                {
                    if (Math.Abs(msg.Time - currTime) < delta && audio.isPlaying && msg.Channel == channel && msg.MessageType == type)
                    {
                        targets.Add(msg);
                    }
                    else
                    {
                        afterConsume.Add(msg);
                    }
                }
                midiMessages = afterConsume;
                return targets;
            }

            public void Refill()
            {
                midiMessages = originalMidiMessages.Select(m => m);
            }
        }
    }
}
