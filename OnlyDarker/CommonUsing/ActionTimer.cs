using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public class ActionTimer : IDisposable
    {
        public bool IsRunning { get; private set; }
        private bool _disposed = false;
        private float _timeLeft;
        public float TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                TimeUpdated?.Invoke(this, EventArgs.Empty);
                if (_timeLeft <= 0)
                {
                    TimeElapsed?.Invoke(this, EventArgs.Empty);
                    IsRunning = false;
                }
            }
        }
        public EventHandler TimeElapsed;
        public EventHandler TimeUpdated;
        public ActionTimer(float time/*, EventHandler elapsedTimeAction, EventHandler updateAction*/)
        {
            TimeLeft = time;
            //TimeUpdated += updateAction;
            //TimeElapsed += elapsedTimeAction;
            IsRunning = true;
        }
        public void Dispose()
        {
            if (_disposed) 
                return;
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
