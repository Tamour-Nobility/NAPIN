using System.Linq;

namespace NPMAPI.ExtensionMethods
{
    public static class Extensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }
    }
}