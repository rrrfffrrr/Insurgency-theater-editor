using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurgency_theater_editor
{
    public struct TheaterBlock
    {
        public bool IsContainer {
            get {
                return (Value == null);
            }
        }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public List<TheaterBlock> Childs { get; private set; }

        public int Type;
        public int Line;

        public TheaterBlock(string key, string value)
        {
            Key = key;
            Value = value;
            Childs = null;
            Line = Type = 0;
        }
        public TheaterBlock(string key, IEnumerable<TheaterBlock> childs)
        {
            Key = key;
            Value = null;
            Childs = new List<TheaterBlock>(childs);
            Line = Type = 0;
        }
    }

    public class TheaterStructure
    {
        public static readonly string[] CATEGORIS = { "ammo", "explosives", "player_settings", "weapons", "weapon_upgrades", "player_gear", "player_templates", "core", "teams" };

        public List<TheaterBlock> Root;
        public TheaterStructure()
        {
            Root = new List<TheaterBlock>();
        }
        public TheaterStructure(string theater) : this()
        {
            Parse(theater);
        }
        public IEnumerator<TheaterBlock> GetEnumerator()
        {
            Stack<TheaterBlock> stack = new Stack<TheaterBlock>();
            for (int i = Root.Count - 1; i >= 0; --i)
            {
                stack.Push(Root[i]);
            }

            while(stack.Count > 0)
            {
                TheaterBlock block = stack.Pop();
                yield return block;
                if (block.IsContainer)
                {
                    for(int i = block.Childs.Count - 1; i >= 0; --i)
                    {
                        stack.Push(block.Childs[i]);
                    }
                }
            }
        }

        public void Parse(string theater)
        {
            Tokenize(theater);
            ValidateToken();
        }
        public void Tokenize(string theater)
        {
            int currentLine = 1;
            theater = theater.Trim();
            Stack<List<TheaterBlock>> stack = new Stack<List<TheaterBlock>>();
            stack.Push(Root);

            string key = null;
            StringBuilder builder = new StringBuilder();
            bool buildingIdentifier = false;
            bool isComment = false;

            while(!string.IsNullOrEmpty(theater))
            {
                char c = theater[0];
                theater = theater.Substring(1);
                switch (c)
                {
                    case '/':
                        if (theater[0] == '/')
                        {
                            isComment = true;
                            theater = theater.Substring(1);
                        }
                        break;
                    case '\"':
                        if (isComment)
                            break;
                        if (buildingIdentifier == false)
                        {
                            buildingIdentifier = true;
                        } else
                        {
                            buildingIdentifier = false;
                            if (key == null)
                            {
                                key = builder.ToString();
                            }
                            else
                            {
                                string value = builder.ToString();
                                TheaterBlock blk = new TheaterBlock(key, value);
                                blk.Line = currentLine;
                                stack.Peek().Add(blk);
                                key = null;
                            }
                            builder.Clear();
                        }
                        break;
                    case '{':
                        if (isComment)
                            break;
                        if (buildingIdentifier)
                            throw new Exception("Invalid syntax(" + currentLine + "): Cannot use brace as identifier.");
                        if (key == null)
                            throw new Exception("Invalid syntax(" + currentLine + "): No identifier for this collection found.");
                        TheaterBlock block = new TheaterBlock(key, new List<TheaterBlock>());
                        block.Line = currentLine;
                        key = null;
                        stack.Peek().Add(block);
                        stack.Push(block.Childs);
                        break;
                    case '}':
                        if (isComment)
                            break;
                        if (buildingIdentifier)
                            throw new Exception("Invalid syntax(" + currentLine + "): Cannot use brace as identifier.");
                        stack.Pop();
                        break;
                    case ' ':
                        if (isComment)
                            break;
                        if (buildingIdentifier)
                        {
                            builder.Append(c);
                            break;
                        }
                        break;
                    case '\t':
                        if (isComment)
                            break;
                        if (buildingIdentifier)
                            throw new Exception("Invalid syntax(" + currentLine + "): Cannot use tab character as identifier.");
                        break;
                    case '\n':
                        isComment = false;
                        currentLine++;
                        if (buildingIdentifier)
                            throw new Exception("Invalid syntax(" + currentLine + "): Cannot use new line character as identifier.");
                        break;
                    default:
                        if (isComment)
                            break;
                        if (buildingIdentifier)
                        {
                            builder.Append(c);
                            break;
                        }
                        break;
                }
            }

            if (stack.Count != 1)
            {
                throw new Exception("Invalid syntax(" + currentLine + "): Number of braces does not match.");
            }
        }
        public void ValidateToken()
        {
            int level = 0;
            for (var iter = GetEnumerator(); iter.MoveNext();)
            {
                TheaterBlock block = iter.Current;
                if (block.IsContainer)
                {
                    switch(level)
                    {
                        case 0:
                            if (block.Key.CompareTo("theater") != 0)
                            {
                                throw new Exception("Syntax error(" + block.Line + "): First collection must be \"theater\".");
                            }
                            break;
                        case 1:
                            bool contain = false;
                            foreach(string cat in CATEGORIS)
                            {
                                if (block.Key.CompareTo(cat) == 0)
                                {
                                    contain = true;
                                    break;
                                }
                            }

                            if (!contain)
                            {
                                StringBuilder builder = new StringBuilder("Syntax error(");
                                builder.Append(block.Line);
                                builder.Append("): Second collection must be one of the next.\n");
                                foreach (string cat in CATEGORIS)
                                {
                                    builder.Append(cat);
                                    builder.Append(", ");
                                }
                                builder.Remove(builder.Length - 2, 2);
                                throw new Exception(builder.ToString());
                            }
                            break;
                        default:
                            break;
                    }
                    level++;
                } else
                {
                    switch(block.Key)
                    {
                        case "#base":
                            if (level > 0)
                                throw new Exception("Syntax error(" + block.Line + "): \"#base\" must be top of file.");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
