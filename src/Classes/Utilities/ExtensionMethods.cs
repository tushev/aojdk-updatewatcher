using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adoptium_UpdateWatcher
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> original)
        {
            return original ?? Enumerable.Empty<T>();
        }

        public static int Remove<T>(
            this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
    }
}
