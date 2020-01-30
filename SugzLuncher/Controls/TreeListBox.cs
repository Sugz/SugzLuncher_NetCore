using SugzLuncher.Helpers;
using SugzLuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SugzLuncher.Controls
{

    public class TreeListBox : ListBox
    {

        #region Overrides

        /// <summary>
        /// Grap template parts and assign event handlers
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AddEventHandlers();
        }


        /// <summary>
        /// Assign items event handlers
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            ListBoxItem item = new ListBoxItem();
            item.MouseEnter += (s, e) => item.IsSelected = true;
            item.Drop += OnItemDrop;
            return item;
        }

        #endregion Overrides


        #region Event Handlers

        /// <summary>
        /// Assign event handlers
        /// </summary>
        private void AddEventHandlers()
        {
            
        }





        /// <summary>
        /// Send the droped data to the viewmodel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemDrop(object sender, DragEventArgs e)
        {
            if (sender is ListBoxItem item && 
                item.DataContext is IDropTarget dropTarget)
            {
                dropTarget.Drop(e.Data);
            }
            

        }

        #endregion Event Handlers

    }

}
