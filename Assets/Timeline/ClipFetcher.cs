using Assets.Timeline.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Timeline
{
    /* Scan Through Assets Folder And Return All Clips */
    class ClipFetcher
    {
        IEnumerable<Clip> clips = new List<Clip>();

        public void Load()
        {
            clips = new List<Clip> { new Clip("Test 1"), new Clip("Test 2") };
        }

        public IEnumerable<Clip> GetClips()
        {
            return clips;
        }
    }
}
