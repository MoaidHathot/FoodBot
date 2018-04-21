using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FoodBot.Model;
using Newtonsoft.Json;

namespace FoodBot
{
    public class OrderItemConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, objectType);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Beverage) || typeof(Hamburger) == objectType || typeof(Pizza) == objectType;
        }


    }
}