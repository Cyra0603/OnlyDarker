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
        public void Update(GameTime gameTime)
        {
            if (!IsRunning) return;
            TimeLeft -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }
        public void Pause() => IsRunning = false;
        public void Unpause() => IsRunning = true;
    }
}
