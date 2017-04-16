using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Midi
{
    public class Midi
    {
        public int NumTracks { get; set; }
        public int Division { get; set; }
        public List<MidiTrack> Tracks { get; set; }
    }
}
