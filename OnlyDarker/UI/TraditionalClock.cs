using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System.Diagnostics;

namespace OnlyDarker.UI
{
    public class TraditionalClock
    {
        public Vector2 Center;
        public Texture2D ClockTexture;
        public float Scale = 1F;
        int _currentHours;
        int _currentMinutes;
        int _currentSeconds;
        int secondsHandLength;
        int hoursHandLength;
        int minutesHandLength;
        DateTime _lastUpdatedTime;
        Line _hoursHand;
        Line _minutesHand;
        Line _secondsHand;

        public TraditionalClock(Vector2 position)
        {
            ClockTexture = TextureMapper.GetInstance().ClockTexture;
            Center = position;
            var time = _lastUpdatedTime = DateTime.Now;
            secondsHandLength = ClockTexture.Width / 2 - 3;
            hoursHandLength = ClockTexture.Width / 4;
            minutesHandLength = secondsHandLength / 2 * 3;
            _secondsHand = new(Center, new(Center.X, Center.Y - secondsHandLength));
            float secondDegrees = 6 * time.Second;
            _secondsHand.EndPoint = _secondsHand.EndPoint.RotateAround(Center, secondDegrees);

            _minutesHand = new(Center, new(Center.X, Center.Y - minutesHandLength));
            float minutesDegrees = 6 * time.Minute;
            _minutesHand.EndPoint = _minutesHand.EndPoint.RotateAround(Center, minutesDegrees);

            _hoursHand = new(Center, new(Center.X, Center.Y - hoursHandLength));
            float hoursDegrees = 30 * time.Hour;
            _hoursHand.EndPoint = _hoursHand.EndPoint.RotateAround(Center, hoursDegrees);
        }

        public void Update()
        {
            var time = DateTime.Now;
            _currentHours = time.Hour;
            _currentMinutes = time.Minute;
            _currentSeconds = time.Second;
            if (_lastUpdatedTime.Hour != _currentHours)
            {
                Debug.Print($"{_currentHours}");
                _hoursHand.EndPoint.Y = Center.Y - hoursHandLength;
                var hours = _currentHours;
                if (hours >= 12)
                {
                    hours -= 12;
                }
                float degrees = 360 / 12 * hours;
                _hoursHand.EndPoint = _hoursHand.EndPoint.RotateAround(Center, degrees);
            }

            if (_lastUpdatedTime.Minute != _currentMinutes)
            {
                Debug.Print($"{_currentMinutes}");
                _minutesHand.EndPoint.Y = Center.Y - minutesHandLength;
                float degrees = 360 / 60 * _currentMinutes;
                _minutesHand.EndPoint = _minutesHand.EndPoint.RotateAround(Center, degrees);
            }

            if (_lastUpdatedTime.Second != _currentSeconds)
            {
                Debug.Print($"{_currentSeconds}");
                _secondsHand.EndPoint.Y = Center.Y - secondsHandLength;
                float degrees = 360 / 60 * _currentSeconds;
                _secondsHand.EndPoint = _secondsHand.EndPoint.RotateAround(Center, degrees);
            }

            _lastUpdatedTime = time;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ClockTexture, Center, null, Color.White, 1F, new(ClockTexture.Width / 2, ClockTexture.Height / 2), Vector2.One, SpriteEffects.None, 1F);
            Vector2 hourPos = new Vector2(Center.X, Center.Y - secondsHandLength).RotateAround(Center, 30F);
            for (int i = 1; i <= 12; i++)
            {
                spriteBatch.DrawString(GlobalUse.MainFont, $"{i}", hourPos, Color.White, 0F, Vector2.Zero, 0.1F, SpriteEffects.None, 0F);
                hourPos = hourPos.RotateAround(Center, 30F);
            }
            spriteBatch.DrawLine(_secondsHand, color: Color.Black);
            spriteBatch.DrawLine(_minutesHand, color: Color.Black, thickness: 2);
            spriteBatch.DrawLine(_hoursHand, color: Color.Black, thickness: 3);
        }
    }

}
