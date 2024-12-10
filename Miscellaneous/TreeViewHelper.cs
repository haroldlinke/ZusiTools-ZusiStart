using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ZusiStart.Miscellaneous
{
#if false
    class TreeViewHelper
    {
        public static TreeViewItem GetTreeViewItem(ItemsControl container,object item)
        {
            if (container == null) return null;

            if(container.DataContext==item)
            {
                return container as TreeViewItem;
            }
        }
    }
#endif
}
