using System;
using UnityEngine;

/// <summary>
/// Mark a method with an integer argument with this to display the argument as an enum popup in the UnityEvent
/// drawer. Use: [EnumAction(typeof(SomeEnumType))]
/// From https://forum.unity.com/threads/ability-to-add-enum-argument-to-button-functions.270817
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EnumActionAttribute : PropertyAttribute
{
    public Type EnumType { get; }
    public EnumActionAttribute(Type enumType) => EnumType = enumType;
}