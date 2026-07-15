using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System.Diagnostics;

namespace OnlyDarker.UI
{
    public class TraditionalClock
    {
        public Vector2 Center;
        public Texture2D ClockTexture;
        public float Scale = 3F;
        private int _currentHours;
        private int _currentMinutes;
        private int _currentSeconds;
        private int _secondsHandLength;
        private int _hoursHandLength;
        private int _minutesHandLength;
        DateTime _lastUpdatedTime;
        Line _hoursHand;
        Line _minutesHand;
        Line _secondsHand;

        public TraditionalClock(Vector2 position)
        {
            ClockTexture = TextureMapper.GetInstance().ClockTexture;
            Center = position;
            var time = _lastUpdatedTime = DateTime.Now;
            _secondsHandLength = (int)(ClockTexture.Width * Scale / 2 - 3);
            _hoursHandLength = (int)(ClockTexture.Width * Scale / 4);
            _minutesHandLength = _secondsHandLength / 3 * 2;

            _secondsHand = new(Center, new(Center.X, Center.Y - _secondsHandLength));
            float secondDegrees = 6 * time.Second;
            _secondsHand.EndPoint = _secondsHand.EndPoint.RotateAround(Center, secondDegrees);

            _minutesHand = new(Center, new(Center.X, Center.Y - _minutesHandLength));
            float minutesDegrees = 6 * time.Minute;
            _minutesHand.EndPoint = _minutesHand.EndPoint.RotateAround(Center, minutesDegrees);

            _hoursHand = new(Center, new(Center.X, Center.Y - _hoursHandLength));
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
                _hoursHand = new(Center, new(Center.X, Center.Y - _hoursHandLength));
                var hours = _currentHours;
                if (hours >= 12)
                {
                    hours -= 12;
                }
                float degrees = 30F * hours;
                _hoursHand.EndPoint = _hoursHand.EndPoint.RotateAround(Center, degrees);
            }

            if (_lastUpdatedTime.Minute != _currentMinutes)
            {
                _minutesHand = new(Center, new(Center.X, Center.Y - _minutesHandLength));
                float degrees = 6F * _currentMinutes;
                _minutesHand.EndPoint = _minutesHand.EndPoint.RotateAround(Center, degrees);
            }

            if (_lastUpdatedTime.Second != _currentSeconds)
            {
                _secondsHand = new(Center, new(Center.X, Center.Y - _secondsHandLength));
                float degrees = 6F * _currentSeconds;
                _secondsHand.EndPoint = _secondsHand.EndPoint.RotateAround(Center, degrees);
            }

            _lastUpdatedTime = time;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var font = GlobalUse.MainFont;
            spriteBatch.Draw(ClockTexture, Center, null, Color.Black, 1F, new(ClockTexture.Width / 2, ClockTexture.Height / 2), Vector2.Zero, SpriteEffects.None, 0);
            Vector2 numberPos = new Vector2(Center.X, Center.Y - _secondsHandLength).RotateAround(Center, 30F);
            for (int i = 1; i <= 12; i++)
            {
                Vector2 origin = font.MeasureString(i.ToString());
                float scale = Scale / 10;
                spriteBatch.DrawString(font, $"{i}", numberPos, Color.White, 0F, origin * scale, scale, SpriteEffects.None, 0F);
                numberPos = numberPos.RotateAround(Center, 30F);
            }
            spriteBatch.DrawLine(_secondsHand, color: Color.Black);
            spriteBatch.DrawLine(_minutesHand, color: Color.Black, thickness: 2);
            spriteBatch.DrawLine(_hoursHand, color: Color.Black, thickness: 3);
        }
    }

}
