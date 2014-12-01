using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

// Testing
using AsysORM.EloqueNET.Query;

namespace AsysORM.Diagnostics
{
    public class ExecTimer
    {
        public static void TimeOperations(string testOperationName, long iterations)
        {
            long nanosecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
            long numIterations = iterations;

            // Define variables for operation statistics. 
            long numTicks = 0;
            long numRollovers = 0;
            long maxTicks = 0;
            long minTicks = Int64.MaxValue;
            int indexFastest = -1;
            int indexSlowest = -1;
            long milliSec = 0;

            Stopwatch time10kOperations = Stopwatch.StartNew();

            // Run the current operation 10001 times. 
            // The first execution time will be tossed 
            // out, since it can skew the average time. 

            for (int i = 0; i <= numIterations; i++)
            {
                long ticksThisTime = 0;
                Stopwatch timePerParse;

                // Start a new stopwatch timer.
                timePerParse = Stopwatch.StartNew();

                //-------------------------------------
                // RUN TEST CODE HERE!
                //-------------------------------------


                // Stop the timer, and save the 
                // elapsed ticks for the operation.

                timePerParse.Stop();
                ticksThisTime = timePerParse.ElapsedTicks;

                // Skip over the time for the first operation, 
                // just in case it caused a one-time 
                // performance hit. 
                if (i == 0)
                {
                    time10kOperations.Reset();
                    time10kOperations.Start();
                }
                else
                {

                    // Update operation statistics 
                    // for iterations 1-10001. 
                    if (maxTicks < ticksThisTime)
                    {
                        indexSlowest = i;
                        maxTicks = ticksThisTime;
                    }
                    if (minTicks > ticksThisTime)
                    {
                        indexFastest = i;
                        minTicks = ticksThisTime;
                    }
                    numTicks += ticksThisTime;
                    if (numTicks < ticksThisTime)
                    {
                        // Keep track of rollovers.
                        numRollovers++;
                    }
                }
            }

            // Display the statistics for 10000 iterations.

            time10kOperations.Stop();
            milliSec = time10kOperations.ElapsedMilliseconds;

            Console.WriteLine();
            Console.WriteLine("{0} Summary:", testOperationName);
            Console.WriteLine("  Slowest time:  #{0}/{1} = {2} ticks", indexSlowest, numIterations, maxTicks);
            Console.WriteLine("  Fastest time:  #{0}/{1} = {2} ticks", indexFastest, numIterations, minTicks);
            Console.WriteLine("  Average time:  {0} ticks = {1} nanoseconds", numTicks / numIterations, (numTicks * nanosecPerTick) / numIterations);
            Console.WriteLine("  Total time looping through {0} operations: {1} milliseconds", numIterations, milliSec);
        }
    }
}
