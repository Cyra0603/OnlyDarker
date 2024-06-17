using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing
{
    public class ActionTimer
    {
        public bool IsRunning { get; private set; }
        public bool Expired { get; private set; }
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
                    Expired = true;
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
        ~ActionTimer()
        {
            Debug.WriteLine($"Timer {GetType} disposed");
        }
        public void Update(float milliseconds)
        {
            if (!IsRunning) return;
            TimeLeft -= milliseconds;
        }
        public void Pause() => IsRunning = false;
        public void Unpause() => IsRunning = true;
    }
    public struct Timer
    {
        private float _timeLeft = 0;
        public float TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                //if (_timeLeft > 0.1)
                //    Debug.WriteLine($"Timer timeleft: {_timeLeft}");
                if (_timeLeft < 0)
                    _timeLeft = 0;
            }
        }
        public Timer(float milliseconds)
        {
            TimeLeft = milliseconds;
        }
        public void Update(float milliseconds)
        {
            TimeLeft -= milliseconds;
        }
    }
}
