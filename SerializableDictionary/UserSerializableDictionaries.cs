﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> { }

[Serializable]
public class StateMonoBehaviourDictionary : SerializableDictionary<string, MonoBehaviour> { }

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<Object, Color> { }

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> { }

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> { }

[Serializable]
public class MyClass {
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> { }