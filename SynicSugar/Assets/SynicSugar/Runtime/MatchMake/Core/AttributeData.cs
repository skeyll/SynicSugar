using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using System;

namespace SynicSugar.MatchMake {
    /// <summary>
    /// Lobby and Member Attribute data
    /// </summary>
    public class AttributeData {
        internal LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;
        
        public string Key;
        //Only one of the following properties will have valid data (depending on 'ValueType')
        public bool? BOOLEAN { get; private set; }
        public int? INT64 { get; private set; } = 0;
        public double? DOUBLE { get; private set; } = 0.0;
        public string STRING { get; private set; }
        public AttributeType ValueType { get; private set; } = AttributeType.String;
        public ComparisonOp ComparisonOperator = ComparisonOp.Equal;
        /// <summary>
        /// Can use bool, int, double and string.
        /// Retrun new whole attribute instanse by GenereteSerssionAttribute<T>(Key, Value, advertiseType).
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value){
            BOOLEAN = value;
            ValueType = AttributeType.Boolean;
        }
        public void SetValue(int value){
            INT64 = value;
            ValueType = AttributeType.Int64;
        }
        public void SetValue(double value){
            DOUBLE = value;
            ValueType = AttributeType.Double;
        }
        public void SetValue(string value){
            STRING = value;
            ValueType = AttributeType.String;
        }
        /// <summary>
        /// Get the value based on the ValueType.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <returns>Return T value, if the match Attribute Type and T. If not, throw Exception.</returns>
        public T GetValue<T>() {
            switch (ValueType) {
                case AttributeType.Boolean:
                    if (typeof(T) == typeof(bool)) {
                        return (T)(object)BOOLEAN.Value;
                    }
                    break;
                case AttributeType.Int64:
                    if (typeof(T) == typeof(int)) {
                        return (T)(object)INT64.Value;
                    }
                    break;
                case AttributeType.Double:
                    if (typeof(T) == typeof(double)) {
                        return (T)(object)DOUBLE.Value;
                    }
                    break;
                case AttributeType.String:
                    if (typeof(T) == typeof(string)) {
                        return (T)(object)STRING;
                    }
                    break;
            }
            throw new InvalidOperationException("AttributeData: This data is null or ValueType and T do not match.");
        }

        /// <summary>
        /// Get type as string.
        /// </summary>
        /// <returns>Return string even if that ValueType is not String. If no data, return empty.</returns>
        public string GetValueAsString(){
            switch(ValueType){
                case AttributeType.Boolean:
                return BOOLEAN.ToString();
                case AttributeType.Int64:
                return INT64.ToString();
                case AttributeType.Double:
                return DOUBLE.ToString();
                case AttributeType.String:
                return STRING;
            }
            return string.Empty;
        }
        /// <summary>
        /// Get specific value from user attributes.
        /// </summary>
        /// <param name="list">User attributes </param>
        /// <param name="Key">Target attribute key(The key from server becomes Upper case)</param>
        /// <returns>Return string even if that ValueType is not String. If no data, return empty.</returns>
        public static string GetValueAsString(List<AttributeData> list, string Key){
            foreach(var attr in list){
                if(string.Compare(attr.Key, Key, true) <= 0){
                    return attr.GetValueAsString();
                }
            }
            return string.Empty;
        }
        public override int GetHashCode(){
            return base.GetHashCode();
        }
    }
}