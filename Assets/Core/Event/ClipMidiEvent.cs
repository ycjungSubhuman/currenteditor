using Assets.Timeline.Utility;
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

        public ClipMidiEvent(String clipName, int channel, Midi.MidiMessage.Type type)
        {
            this.clipName = clipName;
            this.channel = channel;
            this.type = type;
        }

        protected override Dictionary<string, Action> GetUpdates()
        {
            Clip clip = MonoHelper.MonoFindClip(clipName);

            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("MidiEvent"+clipName+channel+type, ()=> 
            {
                if (clip.GetCurrMessages(channel, type).Count() > 0)
                {
                    this.Trigger(Params.Empty);
                }
            }
            );
            return updates;
        }
    }
}
