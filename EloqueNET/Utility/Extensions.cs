using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using AsysORM.EloqueNET.Reflection;
using AsysORM.EloqueNET.Query;

namespace AsysORM.EloqueNET
{
    public static class Extensions
    {

        #region Database column / attribute helpers

        /// <summary>
        /// Checks if a dependency property is marked as an attribute. If so, it means that the
        /// property is mapping a database column/attribute
        /// </summary>
        /// <param name="property">The dependency property to check</param>
        /// <returns>true if the property is registered with ColumnPropertyMetadata, false if not</returns>
        public static bool IsAttribute(this DependencyProperty property)
        {
            return property.DefaultMetadata.GetType() == typeof(ColumnPropertyMetadata);
        }

        /// <summary>
        /// Checks if the property is an attribute and if so, if it is guarded.
        /// See: <see cref="AsysORM.EloqueNET.Reflection.ColumnPropertyMetadata"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsGuardedAttribute(this DependencyProperty property)
        {
            if(property.IsAttribute())
            {
                return (property.DefaultMetadata as ColumnPropertyMetadata).Guarded;
            }

            return false;
        }

        /// <summary>
        /// Returns a string of the operator referred to in the 'Is' enum.
        /// </summary>
        /// <param name="conditionOperator">The operator to get the string representation from</param>
        /// <returns>The operator as a valid SQL syntax string</returns>
        public static string GetOperatorName(this Is conditionOperator)
        {
            switch (conditionOperator)
            {
                case Is.EqualTo:
                    return "=";

                case Is.Between:
                    return "BETWEEN";

                case Is.GreaterThan:
                    return ">";

                case Is.GreaterThanOrEqualTo:
                    return ">=";

                case Is.In:
                    return "IN";

                case Is.LessThan:
                    return "<";

                case Is.LessThanOrEqualTo:
                    return "<=";

                case Is.Like:
                    return "LIKE";

                case Is.NotLike:
                    return "NOT LIKE";

                case Is.NotEqualTo:
                    return "<>";

                case Is.NotNull:
                    return "IS NOT";

                case Is.Null:
                    return "IS";
            }

            throw new AsysORMException("No valid condition operator provided!");
        }

        /// <summary>
        /// Splits a string with spaces before any capital letter
        /// </summary>
        /// <example>ThisIsAString = This Is A String</example>
        /// <remarks>Credits: http://stackoverflow.com/questions/272633/add-spaces-before-capital-letters </remarks>
        /// <param name="text"></param>
        /// <param name="preserveAcronyms"></param>
        /// <returns></returns>
        public static string AddSpacesToSentence(this string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static DbType GetSqlValueType(this object value)
        {
            Type type = value.GetType();
            if (type == typeof(string))
            {
                return System.Data.DbType.String;
            }
            else if (type == typeof(int))
            {
                return System.Data.DbType.Int32;
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                return System.Data.DbType.Double;
            }
            else if (type == typeof(DateTime))
            {
                return System.Data.DbType.DateTime;
            }
            else
            {
                return System.Data.DbType.String;
            }
        }

        #endregion

        #region String extensions

        /// <summary>
        /// Columnizes a string array, making each seperate element the name of a Column object in a ColumnList.
        /// Optionally: if the string array is empty, a '*' column is added
        /// </summary>
        /// <param name="strArray">Array of column names</param>
        /// <param name="addAllColumnIfEmpty">Indicates wheter or not a '*' column should be added if the array is empty.</param>
        /// <returns></returns>
        public static ColumnList ToColumnList(this String[] strArray, bool addAllColumnIfEmpty = false)
        {
            ColumnList list = new ColumnList();
            if (strArray.Length == 0 && addAllColumnIfEmpty)
            {
                list.AddAllColumn();
                return list;
            }

            foreach(String str in strArray)
            {
                list.Add(new Column(str));
            }

            return list;
        }

        /// <summary>
        /// Converts camel case names to snake-case. 
        /// <example>'MyCamelCaseName'  =  'my_camel_case_name'</example>
        /// <remarks>Source of pattern: http://stackoverflow.com/a/4489046/994284 </remarks>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this String str)
        {
            var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", 
                RegexOptions.IgnorePatternWhitespace);
            return r.Replace(str, "_").ToLower();
        }

        /// <summary>
        /// Replaces pattern[i] with replacements[i] in the string
        /// </summary>
        /// <param name="input">String to search in and replace in</param>
        /// <param name="pattern">patterns to look for to replace</param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string PregReplace(this String input, string[] pattern, string[] replacements)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

            for(int i = 0; i < pattern.Length; i++)
            {
                input = Regex.Replace(input, pattern[i], replacements[i]);
            }

            return input;
        }

        /// <summary>
        /// Replaces pattern[i] with replacements[i] in the string for 'count' occurances
        /// </summary>
        /// <param name="input">String to search in and replace in</param>
        /// <param name="pattern">patterns to look for to replace</param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string PregReplace(this String input, string[] pattern, string[] replacements, int count)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

            for (int i = 0; i < pattern.Length; i++)
            {
                Regex rgx = new Regex(pattern[i]);
                input = rgx.Replace(input, replacements[i], count);
            }

            return input;
        }

        /// <summary>
        /// Replaces pattern[i] with replacements[i] in the string for count[i] occurances
        /// </summary>
        /// <param name="input">String to search in and replace in</param>
        /// <param name="pattern">patterns to look for to replace</param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public static string PregReplace(this String input, string[] pattern, string[] replacements, int[] count)
        {
            if (replacements.Length != pattern.Length || replacements.Length != count.Length)
                throw new ArgumentException("Replacement and Pattern Arrays must be balanced");

            for (int i = 0; i < pattern.Length; i++)
            {
                Regex rgx = new Regex(pattern[i]);
                input = rgx.Replace(input, replacements[i], count[i]);
            }

            return input;
        }

        /// <summary>
        /// Replaces 'pattern' with 'replacement' in 'input' string
        /// </summary>
        /// <param name="input">Subject of replacement</param>
        /// <param name="pattern">needle to look for</param>
        /// <param name="replacement">replacement string</param>
        /// <returns></returns>
        public static string PregReplace(this String input, string pattern, string replacement)
        { 
            return Regex.Replace(input, pattern, replacement);
        }

        /// <summary>
        /// Replaces 'pattern' with 'replacement' in 'input' string for 'count' occurances
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string PregReplace(this String input, string pattern, string replacement, int count)
        {
            Regex rgx = new Regex(pattern);
            return rgx.Replace(input, replacement, count);
        }

        #endregion
    }
}
