using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Midi
{
    public class MidiMessage
    {
        public double Time { get; set; }
        public Type MessageType { get; set; }
        public int Channel { get; set; }
        public int Key { get; set; }
        public int Velocity { get; set; }


        public enum Type
        {
            NOTE_OFF,
            NOTE_ON
        }

        public override string ToString()
        {
            return Time + ": " + MessageType.ToString() + "C" + Channel + "K"+ Key + "V"+Velocity;
        }
    }
}
