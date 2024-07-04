using Epic.OnlineServices;
using System.Collections.Generic;
using System;
namespace SynicSugar.MatchMake {
    public static class AttributeDataExtensions {
        /// <summary>
        /// Get the value based on the ValueType from list.
        /// </summary>
        /// <param name="list">Target attirubte list</param>
        /// <param name="Key">Target key</param>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <returns>Return T value, if the match Attribute Type and T. If not, throw Exception.</returns>
        public static T GetValue<T>(this List<AttributeData> list, string Key){
            if(list == null || list.Count == 0){
                throw new InvalidOperationException("AttributeDataList: This list is null or empty.");
            }

            foreach(var attr in list){
                if(string.Compare(attr.Key, Key, true) <= 0){
                    switch (attr.ValueType) {
                        case AttributeType.Boolean:
                            if (typeof(T) == typeof(bool)){
                                return (T)(object)attr.GetValue<bool>();
                            }
                        throw new InvalidOperationException("AttributeDataList: ValueType is Boolean but T is not bool.");
                        case AttributeType.Int64:
                            if (typeof(T) == typeof(int)){
                                return (T)(object)attr.GetValue<int>();
                            }
                        throw new InvalidOperationException("AttributeDataList: ValueType is Int64 but T is not int.");
                        case AttributeType.Double:
                            if (typeof(T) == typeof(double)){
                                return (T)(object)attr.GetValue<double>();
                            }
                        throw new InvalidOperationException("AttributeDataList: ValueType is Double but T is not double.");
                        case AttributeType.String:
                            if (typeof(T) == typeof(string)){
                                return (T)(object)attr.GetValue<string>();
                            }
                        throw new InvalidOperationException("AttributeDataList: ValueType is String but T is not string.");
                    }
                }
            }
            throw new InvalidOperationException("AttributeDataList: List does not include the Key.");
        }
    }
}
