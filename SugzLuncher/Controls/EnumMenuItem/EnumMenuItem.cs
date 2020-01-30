using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SugzLuncher.Controls
{
    //TODO: optimize by avoiding to redo the headers and tag when selectedvalue changed, just update the ischecked property. only redo headers when use description changed
    public class EnumMenuItem : MenuItem
    {

        #region EnumType

        /// <summary>
        /// Gets or sets the type of enum used to create the submenu items.
        /// </summary>
        [Description("Gets or sets the type of enum used to create the submenu items."), Category("Common")]
        public Type EnumType
        {
            get => (Type)GetValue(EnumTypeProperty);
            set => SetValue(EnumTypeProperty, value);
        }

        // DependencyProperty for EnumType
        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType),
            typeof(Type),
            typeof(EnumMenuItem),
            new FrameworkPropertyMetadata(default(Enum), OnEnumTypeChanged, OnCoerceEnumType)
        );

        private static object OnCoerceEnumType(DependencyObject d, object baseValue)
        {
            Type type = baseValue as Type;
            if (!type.IsEnum)
                throw new ArgumentException("EnumType must be of type Enum");

            return baseValue;
        }

        private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EnumMenuItem)d).CreateItems();
        }

        #endregion EnumType 


        #region UseDescription

        /// <summary>
        /// Gets or sets wheter the MenuItem header use the Enum value or their description.
        /// </summary>
        [Description("Gets or sets wheter the MenuItem header use the Enum value or their description."), Category("Common")]
        public bool UseDescription
        {
            get => (bool)GetValue(UseDescriptionProperty);
            set => SetValue(UseDescriptionProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SelectedValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UseDescriptionProperty = DependencyProperty.Register(
            nameof(UseDescription),
            typeof(bool),
            typeof(EnumMenuItem),
            new FrameworkPropertyMetadata(
                false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((EnumMenuItem)d).RefreshItems()));

        #endregion UseDescription 


        #region SelectedValue

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        [Description("Gets or sets the selected value."), Category("Common")]
        public object SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SelectedValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(EnumMenuItem),
            new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((EnumMenuItem)d).RefreshItems()));

        #endregion SelectedValue 


        #region Methods


        /// <summary>
        /// Called when a menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void ItemClick(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).Tag is Enum newValue && !newValue.Equals(SelectedValue))
            {
                SelectedValue = newValue;
            }
        }


        /// <summary>
        /// Called when EnumType is defined, create the submenu items
        /// </summary>
        private void CreateItems()
        {
            if (EnumType == null)
                return;

            foreach (var value in Enum.GetValues(EnumType))
            {
                MenuItem mi = new MenuItem
                {
                    Header = UseDescription ? GetEnumDescription(value) : value.ToString(),
                    Tag = value
                };

                if (value.Equals(SelectedValue))
                {
                    mi.IsChecked = true;
                }

                mi.Click += ItemClick;
                Items.Add(mi);
            }
        }


        /// <summary>
        /// Update the submenu items
        /// </summary>
        private void RefreshItems()
        {
            if (EnumType == null)
                return;

            for (int i = 0; i < Items.Count; i++)
            {
                MenuItem mi = (MenuItem)Items[i];
                var value = Enum.GetValues(EnumType).GetValue(i);
                mi.Header = UseDescription ? GetEnumDescription(value) : value.ToString();
                mi.Tag = value;
                mi.IsChecked = value.Equals(SelectedValue);
            }
        }


        /// <summary>
        /// Called when selected value changed.
        /// </summary>
        //private void RefreshItems()
        //{
        //    Items.Clear();
        //    if (SelectedValue == null)
        //        return;

        //    Type enumType = SelectedValue.GetType();
        //    foreach (var value in Enum.GetValues(enumType))
        //    {
        //        MenuItem mi = new MenuItem
        //        {
        //            Header = UseDescription ? GetEnumDescription(value) : value.ToString(),
        //            Tag = value
        //        };

        //        if (value.Equals(SelectedValue))
        //        {
        //            mi.IsChecked = true;
        //        }

        //        mi.Click += ItemClick;
        //        Items.Add(mi);
        //    }

        //}


        /// <summary>
        /// Get the description from an enum value
        /// </summary>
        private string GetEnumDescription(object value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
                return attributes.First().Description;

            return value.ToString();
        }


        #endregion Methods

    }
}
