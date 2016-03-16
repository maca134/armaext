﻿using System;

namespace armaextconsole
{
    public class ARMAExtException : Exception
    {
        public ARMAExtException()
        {
        }

        public ARMAExtException(string message)
            : base(message)
        {
        }

        public ARMAExtException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
