using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

namespace SynicSugar.MatchMake {
    internal static class EOSLobbyExtensions {
        internal static Epic.OnlineServices.Lobby.AttributeData AsLobbyAttribute(this AttributeData attribute){
            Epic.OnlineServices.Lobby.AttributeData data = new ();
            data.Key = attribute.Key;

            switch (attribute.ValueType){
                case AttributeType.String:
                    data.Value = new AttributeDataValue(){ AsUtf8 = attribute.STRING };
                break;
                case AttributeType.Int64:
                    data.Value = new AttributeDataValue(){ AsInt64 = attribute.INT64 };
                break;
                case AttributeType.Double:
                    data.Value = new AttributeDataValue(){ AsDouble = attribute.DOUBLE };
                break;
                case AttributeType.Boolean:
                    data.Value = new AttributeDataValue(){ AsBool = attribute.BOOLEAN };
                break;
            }

            return data;
        }
        internal static bool IsEqualsLobbyAttribute(this AttributeData self, object otherAttribute){
            AttributeData other = (AttributeData)otherAttribute;

            return self.ValueType == other.ValueType &&
                self.BOOLEAN == other.BOOLEAN &&
                self.INT64 == other.INT64 &&
                self.DOUBLE == other.DOUBLE &&
                self.STRING == other.STRING &&
                self.Key == other.Key &&
                self.Visibility == other.Visibility;
        }
        /// <summary>
        /// Generate SynicSugar Attribute from Epic Attribute
        /// </summary>
        /// <param name="serverData"></param>
        /// <returns></returns>
        internal static AttributeData GenerateLobbyAttribute(Epic.OnlineServices.Lobby.Attribute? serverData){
            AttributeData data = new();
            Epic.OnlineServices.Lobby.AttributeData serverAttribute = (Epic.OnlineServices.Lobby.AttributeData)(serverData?.Data);

            data.Key = serverAttribute.Key;
            //Set Value and Type
            switch (serverAttribute.Value.ValueType){
                case AttributeType.Boolean:
                    data.SetValue((bool)serverAttribute.Value.AsBool);
                    break;
                case AttributeType.Int64:
                    data.SetValue((int)serverAttribute.Value.AsInt64);
                    break;
                case AttributeType.Double:
                    data.SetValue((double)serverAttribute.Value.AsDouble);
                    break;
                case AttributeType.String:
                    data.SetValue(serverAttribute.Value.AsUtf8);
                    break;
                default:
                Logger.LogError("GenerateLobbyAttribute", "Can't set attribute. Please confirm the type.");
                return null;
            }

            return data;
        }
    }
}
