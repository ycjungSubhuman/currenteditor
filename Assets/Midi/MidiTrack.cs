using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Midi
{
    class MidiTrack
    {
        public double Bpm { get; set; }
        public int TimeSignatureNumerator { get; set; }
        public int TimeSignatureDenominatore { get; set; }
        public List<MidiMessage> Messages { get; set; }
    }
}
