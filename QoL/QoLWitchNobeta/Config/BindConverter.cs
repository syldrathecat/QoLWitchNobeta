﻿namespace QoLWitchNobeta.Config;

public interface IBindConverter
{
    string Serialize(object value);
    object Deserialize(string text);
}