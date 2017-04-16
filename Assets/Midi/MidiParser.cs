using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Midi
{
    class MidiParser
    {
        int head = 0;
        State state = State.HEADER_TYPE;

        int format;
        int numTracks;
        int division;

        int tempo;
        int timesig_numer;
        int timesig_denom;
        int timesig_clocks_per_metronome;
        int timesig_num_32_notes;
        int deltatime = 0;

        List<MidiMessage> messages = new List<MidiMessage>();
        List<MidiTrack> tracks = new List<MidiTrack>();

        public Midi Parse(byte[] data)
        {
            while(head < data.Length)
            {
                int len = 0;
                switch(state)
                {
                    case State.HEADER_TYPE:
                        String Mthd = ASCIIEncoding.ASCII.GetString(data.Skip(head).Take(4).ToArray());
                        if (Mthd != "MThd") throw new Exception("MThd not found");
                        head += 4;
                        state = State.HEADER_LENGTH;
                        break;
                    case State.HEADER_LENGTH:
                        len = BitConverter.ToInt32(data.Skip(head).Take(4).Reverse().ToArray(), 0);
                        if (len != 6) throw new Exception("Header Length is not 6 : it is" + len + "head: " + head);
                        head += 4;
                        state = State.HEADER_DATA;
                        break;
                    case State.HEADER_DATA:
                        format = BitConverter.ToInt16(data.Skip(head).Take(2).Reverse().ToArray(), 0);
                        numTracks = BitConverter.ToInt16(data.Skip(head+2).Take(2).Reverse().ToArray(), 0);
                        division = BitConverter.ToInt16(data.Skip(head+4).Take(2).Reverse().ToArray(), 0);
                        UnityEngine.Debug.Assert(division > 0);
                        UnityEngine.Debug.Assert(format == 1);
                        head += 6;
                        state = State.TRACK_TYPE;
                        break;
                    case State.TRACK_TYPE:
                        String Mtrk = ASCIIEncoding.ASCII.GetString(data.Skip(head).Take(4).ToArray());
                        if (Mtrk != "MTrk") throw new Exception("MTrk not found");
                        head += 4;
                        state = State.TRACK_LENGTH;
                        break;
                    case State.TRACK_LENGTH:
                        len = BitConverter.ToInt32(data.Skip(head).Take(4).Reverse().ToArray(), 0);
                        head += 4;
                        state = State.TRACK_DELTATIME;
                        break;
                    case State.TRACK_DELTATIME:
                        int value = data.Skip(head).Take(1).ToArray()[0];
                        if((data[head] & 128) != 0) // MSB is ON ~ read next byte for deltatime
                        {
                            deltatime += (value-128) * 128;
                            head += 1;
                        }
                        else //MSB is OFF ~ End of variable deltatime
                        {
                            deltatime += value;
                            head += 1;
                            state = State.TRACK_EVENT;
                        }
                        break;
                    case State.TRACK_EVENT:
                        int status = data.Skip(head).Take(1).ToArray()[0];
                        int data1 = data.Skip(head + 1).Take(1).ToArray()[0];
                        int data2 = data.Skip(head + 2).Take(1).Reverse().ToArray()[0];
                        if(status == 77) //Start of MTrk
                        {
                            state = State.TRACK_TYPE;
                        }
                        else if(status == 255) //Meta Events
                        {
                            head += 1;
                            len = 0;
                            state = State.TRACK_META;
                        }
                        else if(status == 240 || status == 247) //Sysex
                        {
                            head += 1;
                            len = 0;
                            state = State.TRACK_SYSEX_LENGTH;
                        }
                        else
                        {
                            int upperstatus = status / 16;
                            MidiMessage message = new MidiMessage();
                            switch(upperstatus)
                            {
                                case 8: //NOTE OFF
                                    message.MessageType = MidiMessage.Type.NOTE_OFF;
                                    message.Channel = (status & 15) + 1; //mask status upper 4 bits ... value 0 means Channel #1
                                    message.Key = data1;
                                    message.Velocity = data2;
                                    message.Time = deltatime * (60.0) / ((60000000.0 / tempo) * division) ;
                                    head += 3;
                                    messages.Add(message);
                                    break;
                                case 9: //NOTE ON
                                    message.MessageType = MidiMessage.Type.NOTE_ON;
                                    message.Channel = (status & 15) + 1; //mask status upper 4 bits ... value 0 means Channel #1
                                    message.Key = data1;
                                    message.Velocity = data2;
                                    message.Time = deltatime * (60.0) / ((60000000.0 / tempo) * division) ;
                                    head += 3;
                                    messages.Add(message);
                                    break;
                                case 12://Program change(We don't use this message. Just for length)
                                    head += 2;
                                    break;
                                case 13://Channel Pressure(We don't use this message. Just for length)
                                    head += 2;
                                    break;
                                default:
                                    head += 3;
                                    break;
                            }
                            state = State.TRACK_DELTATIME;
                        }
                        break;
                    case State.TRACK_META:
                        int metatype = data.Skip(head).Take(1).Reverse().ToArray()[0];
                        int metalength = 0;
                        int i = 1;
                        while (true)
                        {
                            int metaval = data.Skip(head + i).Take(1).Reverse().ToArray()[0];
                            if ((data[head] & 128) != 0) // MSB is ON ~ read next byte for length
                            {
                                metalength += (metaval - 128) * 128;
                                i += 1;
                            }
                            else //MSB is OFF ~ End of variable length
                            {
                                metalength += metaval;
                                break;
                            }
                        }
                        if (metatype == 81) // Set Tempo
                        {
                            tempo = BitConverter.ToInt32(data.Skip(head+2).Take(3).Reverse().Concat(new byte[] { 0 }).ToArray(), 0);
                            state = State.TRACK_DELTATIME;
                        }
                        else if(metatype == 88) // TIme Signature
                        {
                            timesig_numer = data.Skip(head + 2).Take(1).Reverse().ToArray()[0];
                            timesig_denom = data.Skip(head + 3).Take(1).Reverse().ToArray()[0];
                            timesig_clocks_per_metronome = data.Skip(head + 4).Take(1).Reverse().ToArray()[0];
                            timesig_num_32_notes = data.Skip(head + 5).Take(1).Reverse().ToArray()[0];
                            state = State.TRACK_DELTATIME;
                        }
                        else if (metatype == 47) //End of track
                        {
                            MidiTrack track = new MidiTrack();
                            track.Bpm = 60000000.0 / tempo;
                            track.Messages = messages;
                            messages = new List<MidiMessage>();
                            track.TimeSignatureDenominatore = timesig_denom;
                            track.TimeSignatureNumerator = timesig_numer;
                            tracks.Add(track);
                            deltatime = 0;
                            state = State.TRACK_TYPE;
                        }
                        else
                        {
                            state = State.TRACK_DELTATIME;
                        }
                        head += metalength + 2;
                        break;
                    case State.TRACK_SYSEX_LENGTH:
                        int sysval = data.Skip(head).Take(1).Reverse().ToArray()[0];
                        if((data[head] & 128) != 0) // MSB is ON ~ read next byte for length
                        {
                            len += (sysval-128) * 128;
                            head += 1;
                        }
                        else //MSB is OFF ~ End of variable length
                        {
                            len += sysval;
                            head += len+1;//skip sysex
                            state = State.TRACK_DELTATIME;
                        }
                        break;
                }
            }
            Midi midi = new Midi();
            midi.Division = division;
            midi.NumTracks = numTracks;
            midi.Tracks = tracks;
            return midi;
        }

        enum State
        {
            HEADER_TYPE,
            HEADER_LENGTH,
            HEADER_DATA,
            TRACK_TYPE,
            TRACK_LENGTH,
            TRACK_DELTATIME,
            TRACK_EVENT,
            TRACK_CHANNEL_VOICE, // We need only Channel voice and Meta Tempo messages.
            TRACK_META,
            TRACK_SYSEX_LENGTH,
        }
    }
}
