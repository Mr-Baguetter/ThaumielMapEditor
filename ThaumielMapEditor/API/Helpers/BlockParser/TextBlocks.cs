using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class TextLengthBlock : BlockBase
    {
        public string Text { get; set; } = string.Empty;

        public override object ReturnExecute()
            => Text.Length;
    }

    public class TextJoinBlock : BlockBase
    {
        public List<string> Strings { get; set; } = [];

        public override object ReturnExecute()
        {
            StringBuilder sb = new();

            foreach (string text in Strings)
            {
                sb.AppendLine(text);
            }

            return sb.ToString();
        }
    }
}