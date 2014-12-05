using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AsysORM.Diagnostics
{
    public class ExecutionTimer : IDisposable
    {

        #region Var

        private readonly Stopwatch _stopWatch = new Stopwatch();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the elapsed time of the stopwatch in milliseconds
        /// </summary>
        public long ExecutionTimeMs
        {
            get { return _stopWatch.ElapsedMilliseconds; }
        }


        public string ExecTimeInNs
        {
            get { return ((double)(_stopWatch.Elapsed.TotalMilliseconds * 1000000)).ToString("0.00 ns"); }
        }

        public string ExecTimeInMs
        {
            get { return ((double)(_stopWatch.Elapsed.TotalMilliseconds)).ToString("0.00 ms"); }
        }

        #endregion

        #region Init

        public ExecutionTimer(bool start = true)
        {
            if(start)
            {
                _stopWatch.Start();
            }
        }

        #endregion

        #region Methods

        public void Pause()
        {
            if (_stopWatch.IsRunning)
            {
                _stopWatch.Stop();
            } 
        }

        public void Resume()
        {
            if(!_stopWatch.IsRunning)
            {
                _stopWatch.Start();
            }
        }

        public void Start()
        {
            _stopWatch.Reset();
            _stopWatch.Start();
        }

        public void Stop()
        {
            _stopWatch.Stop();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
        }

        #endregion
        
    }
}
