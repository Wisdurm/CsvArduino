using System.Collections;

namespace CvsArduino
{
    struct NoteSequence ()
    {
        public List<int> notes = [];
        public List<int> noteStarts = [];
        public List<int> noteLengths = [];
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string[] file = File.ReadAllLines(args[0]);

                //Parse
                List<NoteSequence> noteSequences = [];
                NoteSequence combined = new();

                Dictionary<string, int[]> ongoingNotes = [];

                for (int i = 0; i < file.Length; i++)
                {
                    //Track, Time, Note_on_c, Channel, Note, Velocity
                    //1, 480, Note_on_c, 0, 77, 80
                    string[] parse = file[i].Split(',');
                    if (parse[0] == "Press any key to continue . . . ")
                        continue;
                    if (parse[2].Contains("Note_"))
                    {
                        if (parse[2] == " Note_on_c" && parse[5] != " 0") //Start note
                        {
                            ongoingNotes.Add(parse[4], [ Convert.ToInt32(parse[1]), ongoingNotes.Count]); //Keep track of started notes (key = note, value = startTime and "channel")
                        }
                        else //Stop note
                        {
                            int note = Convert.ToInt32(parse[4]);
                            ongoingNotes.Remove(parse[4], out int[] value); //When a note stops, get it's start info from the dictionary
                            int noteStart = value[0];
                            int channel = value[1];

                            while (noteSequences.Count - 1 < channel)
                                noteSequences.Add(new NoteSequence());

                            noteSequences[channel].notes.Add(note);
                            noteSequences[channel].noteStarts.Add(noteStart);
                            noteSequences[channel].noteLengths.Add(Convert.ToInt32(parse[1]) - noteStart); //End - start

                            combined.notes.Add(note);
                            combined.noteStarts.Add(noteStart);
                            combined.noteLengths.Add(Convert.ToInt32(parse[1]) - noteStart);
                        }
                    }
                }

                for (int i = 0; i < noteSequences.Count; i++)
                {
                    File.WriteAllText($"output/notes{i}.txt", string.Join(",", noteSequences[i].notes));
                    File.WriteAllText($"output/notes{i}S.txt", string.Join(",", noteSequences[i].noteStarts));
                    File.WriteAllText($"output/notes{i}L.txt", string.Join(",", noteSequences[i].noteLengths));
                }
                //Combined
                File.WriteAllText($"output/notesC.txt", string.Join(",", combined.notes));
                File.WriteAllText($"output/notesCS.txt", string.Join(",", combined.noteStarts));
                File.WriteAllText($"output/notesCL.txt", string.Join(",", combined.noteLengths));


                //Play track (debugging)
                //const float noteMultiplier = 1f;
                //const int delayMultiplier = 1;

                //Not only is it impossible to play multiple notes at once using console.beep(), but I was also too lazy to make this 

                //int time = 0;
                //int j = 0;
                //while (j < notes.Length)
                //{
                //    if (noteStarts[j] == time)
                //    {
                //        Console.Beep(MidiToFrequency(notes[j]), (int)(noteLengths[j] * noteMultiplier)); //boohoo intellicode, only works on windows waaaa
                //        j++;
                //        if (noteStarts[j + 1] == time) //Don't crash on simultaneous notes //Ported code from js, this can and will cause an IndexOutOfRangeException, but too lazy to fix
                //            time += noteLengths[j];
                //    }
                //    else
                //    {
                //        time++;
                //        if (time % delayMultiplier == 0)
                //            Thread.Sleep(1);
                //    }
                //}

            }
            else //No file
            {
                Console.WriteLine("No file provided!");
            }
        }
        public static int MidiToFrequency(int MidiNumber) //This works I guess 🤷‍
        {
            return (int)(Math.Pow(2, (MidiNumber - 69) / 12.0) * 440);
        }
    }
}
