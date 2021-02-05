using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NumberOfOccurrences {
    
    /* Console run Arguments :
     * 1st - Path to the Folder
     * 2nd - Word to Count
     */
    
    internal class Program {
        private class State {
            public int NumberOfFiles { get; set; }
            public int NumberOfFilesWithString { get; set; }
            public int NumberOfLinesWithString { get; set; }

            public void Add(State other) {
                this.NumberOfFiles += other.NumberOfFiles;
                this.NumberOfFilesWithString += other.NumberOfFilesWithString;
                this.NumberOfLinesWithString += other.NumberOfLinesWithString;
            }

            public override string ToString() {
                return String.Format(
                    $"Number of Files = {NumberOfFiles}, NumberOfFilesWithString = {NumberOfFilesWithString}, NumberOfLinesWithString = {NumberOfLinesWithString}");
            }
        }

        static State globalState = new State();


        public static void SearchFiles(string directoryName, string stringToSearch) {
            Console.WriteLine($"Entered in file ${directoryName}");
            List<Task> directoriesTasks = new List<Task>();
            State myState = new State();
            object theLock = new object();


            foreach (var enumerateDirectory in Directory.EnumerateDirectories(directoryName)) {
                directoriesTasks.Add(Task.Run( () =>SearchFiles(enumerateDirectory, stringToSearch)));
            }
            
            Parallel.ForEach(
                Directory.EnumerateFiles(directoryName),
                () => new State(),
                (fileName, loopState, partial) => {
                    
                    bool found = false;
                    string line;
                    partial.NumberOfFiles++;
                    using (StreamReader r = new StreamReader(fileName)) {
                        while ((line = r.ReadLine()) != null) {
                            if (line.Contains(stringToSearch)) {
                                found = true;
                                partial.NumberOfLinesWithString++;
                            }
                        }
                    }
                    if (found) partial.NumberOfFilesWithString++;                  
                    return partial;
                },
                (partial) => {
                    lock (theLock) {
                        myState.Add(partial);
                    }
                }
            );
            lock (globalState) {
                globalState.Add(myState);
            }

            try {
                Task.WaitAll(directoriesTasks.ToArray());
            }
            catch (AggregateException ae) {
                throw ae.InnerExceptions[0];
            }
        }


        public static void Main(string[] args) {
            string path = args[0];
            string word = args[1];
            SearchFiles(path, word);
            Console.WriteLine(globalState);
        }
    }
}
