using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml;
using System.Windows;


namespace XMLEditor.Common
{
    public class TreeViewHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                return (double)value - 50;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class XmlAttributesToLableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            XmlAttributeCollection attributes = value as XmlAttributeCollection;
            if (attributes != null)
            {
                if (attributes.Count == 1)  
                    return ": " + attributes[0].Value;
                if (attributes.Count == 2)
                    return ": " + attributes[0].Value + " : " + attributes[1].Value;
                //Step to store name attribute if present.
                foreach (XmlAttribute item in attributes)
                {
                    if (item != null)
                    {
                        if (string.Compare(item.Name, "name", true) == 0)
                        {
                            return ": " + item.Value;
                        } 
                        else if (string.Compare(item.Name, "type", true) == 0)
                        {
                            return ": " + item.Value;
                        }
                    }
                }

            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BoolToVisibilityInverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                return ((string)value).Length > 0;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AddChildVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            XmlNode node = value as XmlNode;

            //If Node is text type then do not show add.
            if (node != null && ((node.NodeType == XmlNodeType.Text) || (node.FirstChild != null && node.FirstChild.NodeType == XmlNodeType.Text)))
            {
                return Visibility.Collapsed;
            }
            else
            //If Node is of other type show add
            {
                return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RemoveChildVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            XmlNode node = value as XmlNode;

            //If Node is text type or contains more than one attribute then  show remove.
            if (node != null && (node.NodeType == XmlNodeType.Text || (node.Attributes != null && node.Attributes.Count > 0)))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
