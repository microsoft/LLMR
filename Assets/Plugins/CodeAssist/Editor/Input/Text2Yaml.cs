using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Input
{

    public class Text2Yaml
    {
        public static string Convert(IEnumerable<string> textLines)
        {
            var sb = new StringBuilder();
            var stack = new Stack<(string typeName, string identifier, int indentation)>();

            sb.AppendLine(@"%YAML 1.1");
            sb.AppendLine(@"%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine(@"--- !u!13 &1");
            sb.AppendLine(@"InputManager:");

            var regexIndentation = new Regex("^\\s*");

            var regexString = new Regex("^(\\s+)(\\w+)\\s+\"([a-zA-Z0-9_ ]*)\"\\s+\\(string\\)$");
            var regexValue = new Regex("^(\\s+)(\\w+)\\s+(-?[0-9.]*)\\s+\\(((bool)|(int)|(float)|(unsigned int))\\)$");
            var regexType = new Regex("^(\\s+)(\\w+)\\s+\\((\\w+)\\)$");

            var regexVectorSize = new Regex("(\\s+)size\\s+(\\d)+\\s+\\(int\\)");
            //var regexVectorData = new Regex("(\\s+)data  \\(InputAxis\\)"); // remove InputAxis to make it more generic

            string curTextLine;
            var curTextLineNo = 3;
            var textIndentation = 1;
            var indentationPrefix = new string(' ', textIndentation * 2);
            stack.Push(("InputManager", "InputManager", textIndentation));


            foreach (var line in textLines.Skip(4))
            {
                curTextLine = line;
                curTextLineNo++;


                // Skip empty lines
                if (line.Length == 0)
                    continue;

                // Check if type undeclared, scope goes down, indentation decrements
                {
                    var indentationMatch = regexIndentation.Match(line);
                    if (indentationMatch.Success)
                    {
                        var indentation = indentationMatch.Groups[0].Value.Length;

                        if (indentation > textIndentation)
                            Error($"indentation({indentation}) > textIndentation({textIndentation})");

                        while (indentation < textIndentation)
                        {
                            stack.Pop();
                            textIndentation--;
                            var typeIndentation = textIndentation;
                            if (stack.TryPeek(out var curType2))
                                typeIndentation = curType2.indentation;
                            else if (line.Length > 0)
                                Error("stack empty at type undeclaration");
                            indentationPrefix = new string(' ', typeIndentation * 2);
                        }

                    }
                    else
                    {
                        Error($"{nameof(regexIndentation)} failed");
                    }
                }

                // Skip size field of vectors
                if (stack.TryPeek(out var curType1) && curType1.typeName == "vector")
                {
                    var vectorSizeMatch = regexVectorSize.Match(line);
                    if (vectorSizeMatch.Success)
                    {
                        continue;
                    }
                }

                // Read string fields
                {
                    var stringMatch = regexString.Match(line);
                    if (stringMatch.Success)
                    {
                        AddLine(stringMatch.Groups[2] + ": " + stringMatch.Groups[3]);
                        continue;
                    }
                }

                // Read bool/int/float/unsignedInt fields
                {
                    var valueMatch = regexValue.Match(line);
                    if (valueMatch.Success)
                    {
                        AddLine(valueMatch.Groups[2] + ": " + valueMatch.Groups[3]);
                        continue;
                    }
                }

                // Check if new type declared, scope goes up, indentation increases
                {
                    var typeMatch = regexType.Match(line);
                    if (typeMatch.Success)
                    {
                        var identifier = typeMatch.Groups[2].Value;
                        var typeName = typeMatch.Groups[3].Value;

                        var isVectorData = false;
                        if (stack.TryPeek(out var curType2) && curType2.typeName == "vector" && identifier == "data")
                            isVectorData = true;

                        var typeIndentation = textIndentation;
                        if (stack.TryPeek(out var curType3))
                            typeIndentation = curType3.indentation;
                        else if (line.Length > 0)
                            Error("stack empty at type declaration");

                        if (!isVectorData)
                        {
                            AddLine(typeMatch.Groups[2] + ":");
                        }
                        else
                        {
                            var customIndentation = typeIndentation - 1;
                            if (customIndentation < 0)
                                Error($"customIndentation({customIndentation}) < 0");
                            var customIndentationPrefix = new string(' ', customIndentation * 2);
                            AddLine("- serializedVersion: 3", customIndentationPrefix);
                        }


                        textIndentation++;
                        typeIndentation++;

                        if (isVectorData)
                            typeIndentation--;

                        stack.Push((typeName, identifier, typeIndentation));
                        indentationPrefix = new string(' ', typeIndentation * 2);

                        continue;
                    }
                }


                Error("line failed to match all cases");

            }



            return sb.ToString();


            void AddLine(string line, string? customIndentationPrefix = null)
            {
                string suffix;
                if (stack.TryPeek(out var top))
                    suffix = $" # {textIndentation}, {top.indentation}, {top.typeName} {top.identifier}";
                else
                    suffix = $" # {textIndentation}, nil";

                if (customIndentationPrefix != null)
                    sb.AppendLine(customIndentationPrefix + line + suffix);
                else
                    sb.AppendLine(indentationPrefix + line + suffix);
            }

            void Error(string message)
            {
                var errorMessage = $"Text2Yaml error '{message}' at lineNo: {curTextLineNo}, line: '{curTextLine}' at {Environment.StackTrace}";
                //throw new Exception(errorMessage);
                Serilog.Log.Warning(errorMessage);
            }

        }


    }

    public static partial class Extensions
    {
        public static bool TryPeek<T>(this Stack<T> stack, /*[MaybeNullWhen(false)]*/ out T result)
        {
            if (stack.Count > 0)
            {
                result = stack.Peek();
                return true;
            }
            else
            {
                result = default!;
                return false;
            }
        }
    }

}