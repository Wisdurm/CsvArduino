using System.Collections;

namespace CvsArduino
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string[] file = File.ReadAllLines(args[0]);

                //Parse
                List<int> notesl = [];
                List<int> noteStartsl = [];
                List<int> noteLengthsl = [];

                Dictionary<string, int> ongoingNotes = [];

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
                            ongoingNotes.Add(parse[4], Convert.ToInt32(parse[1])); //Keep track of started notes (key = note, value = start)
                        }
                        else //Stop note
                        {
                            int note = Convert.ToInt32(parse[4]);
                            ongoingNotes.Remove(parse[4], out int noteStart); //When a note stops, get it's start info from the dictionary

                            notesl.Add(note);
                            noteStartsl.Add(noteStart);
                            noteLengthsl.Add(Convert.ToInt32(parse[1]) - noteStart); //End - start
                        }
                    }
                }

                int[] notes = [.. notesl];
                int[] noteStarts = [.. noteStartsl];
                int[] noteLengths = [.. noteLengthsl];

                File.WriteAllText("output/notes.txt", string.Join(",", notes));
                File.WriteAllText("output/notesS.txt", string.Join(",", noteStarts));
                File.WriteAllText("output/notesL.txt", string.Join(",", noteLengths));


                //Play track (debugging)
                const float noteMultiplier = 1f;
                const int delayMultiplier = 1;

                int time = 0;
                int j = 0;
                while (j < notes.Length)
                {
                    if (noteStarts[j] == time)
                    {
                        Console.Beep(MidiToFrequency(notes[j]), (int)(noteLengths[j] * noteMultiplier)); //boohoo intellicode, only works on windows waaaa
                        j++;
                        if (noteStarts[j + 1] == time) //Don't crash on simultaneous notes //Ported code from js, this can and will cause an IndexOutOfRangeException, but too lazy to fix
                            time += noteLengths[j];
                    }
                    else
                    {
                        time++;
                        if (time % delayMultiplier == 0)
                            Thread.Sleep(1);
                    }
                }

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
