using System.Collections.Generic;

namespace SynicSugar.MatchMake{
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    internal class MemberState {
        public List<AttributeData> Attributes { get; internal set; } = new List<AttributeData>();    
        public RTCState RTCState { get; internal set; } = new RTCState();

        internal AttributeData GetAttributeData(string Key){
            foreach (var attribute in Attributes){
                if(attribute.Key == Key){
                    return attribute;
                }
            }
            //No data
            return null;
        }
    }
}