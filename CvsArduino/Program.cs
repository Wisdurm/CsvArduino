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
                bool noteOn = false;
                int note = 0;
                int noteStart = 0;

                for (int i = 0; i < file.Length; i++)
                {
                    //1, 480, Note_on_c, 0, 77, 80
                    string[] parse = file[i].Split(',');
                    if (parse[0] == "Press any key to continue . . . ")
                        continue;
                    if (parse[2].Contains("Note_"))
                    {
                        if (parse[2] == " Note_on_c" && !noteOn) //Start note
                        {
                            noteOn = true;
                            noteStart = Convert.ToInt32(parse[1]);
                            note = Convert.ToInt32(parse[4]);
                        }
                        else //Stop note
                        {
                            noteOn = false;
                            notesl.Add(note);
                            noteStartsl.Add(noteStart);
                            noteLengthsl.Add(Convert.ToInt32(parse[1]) - noteStart); //End - start
                        }
                    }
                }

                int[] notes = notesl.ToArray();
                int[] noteStarts = noteStartsl.ToArray();
                int[] noteLengths = noteLengthsl.ToArray();

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
                        Console.Beep(MidiToFrequency(notes[j]), (int)(noteLengths[j] * noteMultiplier));
                        time += noteLengths[j];
                        j++;
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
