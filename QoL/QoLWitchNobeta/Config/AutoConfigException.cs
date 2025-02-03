using System;

namespace QoLWitchNobeta.Config;

public abstract class AutoConfigException : Exception
{
    protected AutoConfigException(string message) : base(message)
    {

    }
}