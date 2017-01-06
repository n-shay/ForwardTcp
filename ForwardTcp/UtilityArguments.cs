namespace ForwardTcp
{
    public class UtilityArguments : InputArguments
    {
        public ushort? InboundPort => this.GetUnsignedInt16("in");

        public string OutboundIpAddress1 => this.GetString("out1");

        public string OutboundIpAddress2 => this.GetString("out2");

        public string OutboundIpAddress3 => this.GetString("out3");

        public string OutboundIpAddress4 => this.GetString("out4");

        public string OutboundIpAddress5 => this.GetString("out5");

        public UtilityArguments(string[] args)
            : base(args)
        {
        }

        private ushort? GetUnsignedInt16(string key)
        {
            string adjustedKey;
            if (this.ContainsKey(key, out adjustedKey))
            {
                ushort res;
                ushort.TryParse(this.ParsedArguments[adjustedKey], out res);
                return res;
            }
            return null;
        }

        private uint? GetUnsignedInt32(string key)
        {
            string adjustedKey;
            if (this.ContainsKey(key, out adjustedKey))
            {
                uint res;
                uint.TryParse(this.ParsedArguments[adjustedKey], out res);
                return res;
            }
            return null;
        }

        private string GetString(string key)
        {
            string adjustedKey;
            return this.ContainsKey(key, out adjustedKey)
                ? this.ParsedArguments[adjustedKey]
                : null;
        }

        private bool GetBoolean(string key)
        {
            return this.Contains(key);
        }
    }
}
