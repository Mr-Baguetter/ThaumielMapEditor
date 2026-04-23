using System;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Serialization.Converters
{
    public class FloatTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(float);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            Scalar scalar = parser.Consume<Scalar>();
            return float.Parse(scalar.Value, CultureInfo.InvariantCulture);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            => ReadYaml(parser, type);

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            float floatValue = (float?)value ?? 0f;
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, floatValue.ToString("0.##########", CultureInfo.InvariantCulture), ScalarStyle.Plain, true, false));
        }

        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
            => WriteYaml(emitter, value, type);
    }
}