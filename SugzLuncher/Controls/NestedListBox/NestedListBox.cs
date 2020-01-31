using SugzLuncher.Helpers;
using SugzLuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SugzLuncher.Controls
{
    [Flags]
    public enum NestedListBoxDropSide
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Center = 16,
    }


    public class NestedListBox : ListBox
    {

        #region Fields

        private DispatcherTimer _Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        private StackPanel _PART_Container;
        private ItemsPresenter _PART_Items;
        private Border _PART_DropIndicator;

        #endregion Fields



        #region Fields

        public NestedListBox ParentListBox { get; private set; }
        public NestedListBox ChildListBox { get; private set; }

        #endregion Fields



        #region Overrides

        /// <summary>
        /// Grap template parts and assign event handlers
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Container") is StackPanel sp)
                _PART_Container = sp;

            if (Template.FindName("PART_Items", this) is ItemsPresenter ip)
                _PART_Items = ip;

            if (GetTemplateChild("PART_DropIndicator") is Border bd)
                _PART_DropIndicator = bd;

            AddEventHandlers();
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            // To avoid infinite recursion on adding a listBox to a listBox, 
            // the child listbox is only added when first needed
            if (ChildListBox is null)
                AddChildListBox();

            // Check if ChildListBox ItemTemplate && ItemsSource need to be updated
            if (e.RemovedItems.Count > 0 && SelectedItem?.GetType() != e.RemovedItems[0].GetType())
                SetChildListBoxItemTemplate();

            SetChildListBoxVisibility();
        }


        /// <summary>
        /// Assign items event handlers
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            ListBoxItem item = new ListBoxItem();
            item.MouseEnter += (s, e) => item.IsSelected = true;
            item.DragOver += OnItemDragOver;
            item.Drop += OnItemDrop;
            return item;
        }

        #endregion Overrides



        



        #region Constructor

        static NestedListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NestedListBox), new FrameworkPropertyMetadata(typeof(NestedListBox)));
        }


        public NestedListBox()
        {
            
        }

        #endregion Constructor



        #region Event Handlers

        /// <summary>
        /// Assign event handlers
        /// </summary>
        private void AddEventHandlers()
        {
            _Timer.Tick += (s, e) => Deselect();
            //MouseEnter += (s, e) => OnMouseEnter();
            _PART_Items.MouseEnter += (s, e) => OnMouseEnter();
            _PART_Items.DragEnter += (s, e) => OnMouseEnter();
            _PART_Items.MouseLeave += (s, e) => SetForDeselect();
            _PART_Items.DragLeave += (s, e) => SetForDeselect();
        }

        



        /// <summary>
        /// 
        /// </summary>
        private void OnMouseEnter()
        {
            StopTimers();
            //SetBackground(true);

            if (ChildListBox != null)
                ChildListBox.SelectedItem = null;
        }


        /// <summary>
        /// Set the drop indicator position and the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemDragOver(object sender, DragEventArgs e)
        {
            StopTimers();

            if (_PART_DropIndicator != null && sender is ListBoxItem item)
            {
                NestedListBoxDropSide sides = GetSidesAtPoint(item, e.GetPosition(item), 0.2);

                if ((sides & NestedListBoxDropSide.Top) != 0 || 
                    (sides & NestedListBoxDropSide.Bottom) != 0)
                {
                    item.IsSelected = false;
                    SetDropIndicator(item, sides);
                }
                else
                {
                    item.IsSelected = true;
                    SetDropIndicator(null);
                }
            }
        }


        /// <summary>
        /// Send the droped data to the viewmodel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemDrop(object sender, DragEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                NestedListBoxDropSide sides = GetSidesAtPoint(item, e.GetPosition(item), 0.2);
                if (((sides & NestedListBoxDropSide.Top) != 0 ||
                    (sides & NestedListBoxDropSide.Bottom) != 0) &&
                    DataContext is IDropTarget dropTarget)
                {
                    int index = Items.IndexOf(item.DataContext);

                    if ((sides & NestedListBoxDropSide.Bottom) != 0)
                        index++;

                    dropTarget.Drop(e.Data, index);
                }
                else if (item.DataContext is IDropTarget itemDropTarget)
                {
                    itemDropTarget.Drop(e.Data);
                }

                if (ChildListBox.Visibility != Visibility.Visible)
                    SetChildListBoxVisibility();
            }

            SetDropIndicator(null);
        }

        #endregion Event Handlers



        #region Methods

        /// <summary>
        /// Add the next ListBox on demand
        /// </summary>
        private void AddChildListBox()
        {
            ChildListBox = new NestedListBox {
                ParentListBox = this,
                Background = Background,
                ItemTemplate = ItemTemplate,
                ItemTemplateSelector = ItemTemplateSelector,
                Resources = Resources //TODO: Check that, might be cool
            };

            ChildListBox.SetBinding(DataContextProperty, new Binding("SelectedItem") { Source = this });

            SetChildListBoxItemTemplate();

            _PART_Container.Children.Add(ChildListBox);
        }


        /// <summary>
        /// 
        /// </summary>
        private void SetChildListBoxItemTemplate()
        {
            if (ItemTemplate is HierarchicalDataTemplate hTemplate)
                ChildListBox.SetBinding(ItemsSourceProperty, hTemplate.ItemsSource);

            if (ItemTemplateSelector?.SelectTemplate(SelectedItem, this) is HierarchicalDataTemplate _hTemplate)
                ChildListBox.SetBinding(ItemsSourceProperty, _hTemplate.ItemsSource);
        }


        /// <summary>
        /// Show and place or hide the ChildListBox depending on the SelectedItem children
        /// </summary>
        private void SetChildListBoxVisibility()
        {
            if (ChildListBox.HasItems &&
                ItemContainerGenerator.ContainerFromItem(SelectedItem) is ListBoxItem item)
            {
                ChildListBox.Visibility = Visibility.Visible;
                ChildListBox.Margin = new Thickness(0, GetItemYLocation(item), 0, 0);
            }
            else
            {
                ChildListBox.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// Get an item Y location
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private double GetItemYLocation(ListBoxItem item) =>
            item.TranslatePoint(new Point(0, 0), this).Y;


        /// <summary>
        /// Get over which part of an item a point is.
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="p"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private NestedListBoxDropSide GetSidesAtPoint(FrameworkElement fe, Point p, double size = 0.5)
        {
            NestedListBoxDropSide sides = 0;

            if (p.X >= 0 && p.X <= fe.ActualWidth * size)
                sides |= NestedListBoxDropSide.Left;
            if (p.Y >= 0 && p.Y <= fe.ActualHeight * size)
                sides |= NestedListBoxDropSide.Top;
            if (p.X >= fe.ActualWidth - (fe.ActualWidth * size) && p.X <= fe.ActualWidth)
                sides |= NestedListBoxDropSide.Right;
            if (p.Y >= fe.ActualHeight - (fe.ActualHeight * size) && p.Y <= fe.ActualHeight)
                sides |= NestedListBoxDropSide.Bottom;

            if (sides == 0)
                sides = NestedListBoxDropSide.Center;

            return sides;
        }


        /// <summary>
        /// Set the drop indicator based on the hovered item and the mouse position relative to the item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="sides"></param>
        public void SetDropIndicator(ListBoxItem item, NestedListBoxDropSide sides = NestedListBoxDropSide.Center)
        {
            if (_PART_DropIndicator != null)
            {
                double opacity = 0;
                if (item != null)
                {
                    opacity = 1;
                    double yMargin = GetItemYLocation(item);
                    int index = ItemContainerGenerator.IndexFromContainer(item);

                    if (index == 0 && (sides & NestedListBoxDropSide.Bottom) != 0)
                    {
                        yMargin += item.ActualHeight - _PART_DropIndicator.ActualHeight / 2;
                    }

                    else if (index == Items.Count - 1 && (sides & NestedListBoxDropSide.Bottom) != 0)
                    {
                        yMargin += item.ActualHeight - _PART_DropIndicator.ActualHeight;
                    }

                    else if (index != 0)
                    {
                        if ((sides & NestedListBoxDropSide.Bottom) != 0)
                            yMargin += item.ActualHeight - _PART_DropIndicator.ActualHeight / 2;
                        else
                            yMargin -= _PART_DropIndicator.ActualHeight / 2;
                    }

                    _PART_DropIndicator.Margin = new Thickness(0, yMargin, 0, 0);
                }

                DoubleAnimation animation = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(100));
                _PART_DropIndicator.BeginAnimation(OpacityProperty, animation);
            }
        }


        /// <summary>
        /// Prepare to return to idle state, either straight away or with delay
        /// </summary>
        private void SetForDeselect()
        {
            if (!_PART_Items.IsMouseOver)
            {
                if (ChildListBox.HasItems)
                    _Timer.Start();
                else
                    Deselect();

                ParentListBox?.SetForDeselect();
            }
        }


        /// <summary>
        /// Return to idle state
        /// </summary>
        private void Deselect()
        {
            SetDropIndicator(null);
            SelectedItem = null;
            _Timer.Stop();
        }


        /// <summary>
        /// Recursively stop parent ListBoxes from returning to idle state
        /// </summary>
        private void StopTimers()
        {
            _Timer.Stop();
            ParentListBox?.StopTimers();
        }


        #endregion Methods

    }
}
