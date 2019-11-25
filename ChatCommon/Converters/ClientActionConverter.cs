using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatCommon.Actions;

namespace ChatCommon.Converters
{
    //public class ClientActionConverter : JsonConverter<ClientAction>
    //{
        //public override DateTimeOffset Read(
        //    ref Utf8JsonReader reader,
        //    Type typeToConvert,
        //    JsonSerializerOptions options) =>
        //    DateTimeOffset.ParseExact(reader.GetString(),
        //        "MM/dd/yyyy", CultureInfo.InvariantCulture);

        //public override void Write(
        //    Utf8JsonWriter writer,
        //    DateTimeOffset value,
        //    JsonSerializerOptions options) =>
        //    writer.WriteStringValue(value.ToString(
        //        "MM/dd/yyyy", CultureInfo.InvariantCulture));
    //}
}