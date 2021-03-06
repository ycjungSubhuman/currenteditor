﻿using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Event
{
    class ClipMidiEvent : EventPromise
    {
        String clipName;
        int channel;
        Midi.MidiMessage.Type type;
        Clip clip;
        Clip.ClipTimeTracker tracker;

        public ClipMidiEvent():base(Params.Empty) { }

        public ClipMidiEvent(Params ps)
            :base(ps)
        {
            this.clipName = ps.GetString("ClipName");
            this.channel = ps.GetInt("Channel");
            this.type = (Midi.MidiMessage.Type)Enum.Parse(typeof(Midi.MidiMessage.Type), ps.GetString("MessageType"));
            this.clip = MonoHelper.MonoFindClip(clipName);
            this.tracker = clip.GetTracker();
        }

        public override void Reset()
        {
            base.Reset();
            this.tracker = clip.GetTracker();
        }

        public override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("MidiEvent"+clipName+channel+type, ()=> 
            {
                if (tracker.GetCurrMessages(channel, type).Count() > 0)
                {
                    this.Trigger(Params.Empty);
                }
            }
            );
            return updates;
        }

        public override Dictionary<string, string> GetDefaultParams()
        {
            return new Dictionary<string, string>()
            {
                { "ClipName", "" },
                { "Channel", "1" },
                { "MessageType", Midi.MidiMessage.Type.NOTE_ON.ToString() },
            };
        }
    }
}
