using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class MessageBox
    {
        private static MessageBox _instance;
        MessageBox()
        {
            _instance = this;
        }
        public static MessageBox GetInstance()
        {
            if (_instance is not null)
                return _instance;
            return new MessageBox();
        }
    }
}
