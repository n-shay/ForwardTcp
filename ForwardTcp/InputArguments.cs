namespace ForwardTcp
{
    using System;
    using System.Collections.Generic;

    public class InputArguments
    {
        #region fields & properties

        public const string Default_Key_Leading_Pattern = "-";

        protected readonly Dictionary<string, string> ParsedArguments =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string this[string key]
        {
            get { return this.GetValue(key); }
            set
            {
                if (key != null)
                    this.ParsedArguments[key] = value;
            }
        }

        public string KeyLeadingPattern { get; }

        #endregion

        #region public methods

        public InputArguments(string[] args, string keyLeadingPattern)
        {
            this.KeyLeadingPattern = !string.IsNullOrEmpty(keyLeadingPattern)
                ? keyLeadingPattern
                : Default_Key_Leading_Pattern;

            if (args != null && args.Length > 0)
                this.Parse(args);
        }

        public InputArguments(string[] args)
            : this(args, null)
        {
        }

        public bool Contains(string key)
        {
            string adjustedKey;
            return this.ContainsKey(key, out adjustedKey);
        }

        public virtual string GetPeeledKey(string key)
        {
            return this.IsKey(key)
                ? key.Substring(this.KeyLeadingPattern.Length)
                : key;
        }

        public virtual string GetDecoratedKey(string key)
        {
            return !this.IsKey(key)
                ? (this.KeyLeadingPattern + key)
                : key;
        }

        public virtual bool IsKey(string str)
        {
            return str.StartsWith(this.KeyLeadingPattern);
        }

        #endregion

        #region internal methods

        protected virtual void Parse(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == null) continue;

                string key = null;
                string val = null;

                if (this.IsKey(args[i]))
                {
                    key = args[i];

                    if (i + 1 < args.Length && !this.IsKey(args[i + 1]))
                    {
                        val = args[i + 1];
                        i++;
                    }
                }
                else
                    val = args[i];

                // adjustment
                if (key == null)
                {
                    key = val;
                    val = null;
                }
                this.ParsedArguments[key] = val;
            }
        }

        protected virtual string GetValue(string key)
        {
            string adjustedKey;
            if (this.ContainsKey(key, out adjustedKey))
                return this.ParsedArguments[adjustedKey];

            return null;
        }

        protected virtual bool ContainsKey(string key, out string adjustedKey)
        {
            adjustedKey = key;

            if (this.ParsedArguments.ContainsKey(key))
                return true;

            if (this.IsKey(key))
            {
                var peeledKey = this.GetPeeledKey(key);
                if (this.ParsedArguments.ContainsKey(peeledKey))
                {
                    adjustedKey = peeledKey;
                    return true;
                }
                return false;
            }

            var decoratedKey = this.GetDecoratedKey(key);
            if (this.ParsedArguments.ContainsKey(decoratedKey))
            {
                adjustedKey = decoratedKey;
                return true;
            }
            return false;
        }

        #endregion

    }
}