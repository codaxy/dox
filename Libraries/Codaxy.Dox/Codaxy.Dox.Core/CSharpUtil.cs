
namespace Codaxy.Dox
{
    public class CSharpUtil : Util
    {
        public static string TrimTypeName(string name)
        {			
			var indexOfDot = name.LastIndexOf(".");
			return indexOfDot != -1 ? name.Substring(indexOfDot+1) : name;			
        }
    }
}