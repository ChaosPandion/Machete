﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Runtime.RuntimeTypes.LanguageTypes;
using Machete.Interfaces;

namespace Machete.Runtime.NativeObjects
{
    public sealed class NRegExp : LObject
    {
        public NRegExp(IEnvironment environment)
            : base(environment)
        {

        }

        public bool Match(string input, int index)
        {
            throw new NotImplementedException();
        }
    }
}
