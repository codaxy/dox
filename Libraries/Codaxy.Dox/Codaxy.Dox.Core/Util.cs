namespace Codaxy.Dox
{
    public class Util
    {
        public static string ExtractMethodName(string name)
        {
            if (name != null)
            {
                var index = name.IndexOf('(');
                if (index != -1)
                    return name.Substring(0, index);
            }
            return name;
        }

        public static string ExtractTemplateName(string name)
        {
            if (name != null)
            {
                var index = name.IndexOf('<');
                if (index != -1)
                    return name.Substring(0, index);
            }
            return name;
        }
    }
}