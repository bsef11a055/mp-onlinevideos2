﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.UI.SkinEngine.MarkupExtensions;
using OnlineVideos.Sites;

namespace OnlineVideos.MediaPortal2
{
    public class SiteUtilIconConverter : IValueConverter
    {
        public bool Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            result = null;
            if (value is SiteUtilBase)
            {
                string subDir = string.IsNullOrEmpty(parameter as string) ? "Icons" : parameter as string;
                // use Icon with the same name as the Site
                string image = System.IO.Path.Combine(OnlineVideoSettings.Instance.ThumbsDir, subDir + @"\" + ((SiteUtilBase)value).Settings.Name + ".png");
                if (System.IO.File.Exists(image)) result = image;
                else
                {
                    // if that does not exist, try icon with the same name as the Util
                    image = System.IO.Path.Combine(OnlineVideoSettings.Instance.ThumbsDir, subDir + @"\" + ((SiteUtilBase)value).Settings.UtilName + ".png");
                    if (System.IO.File.Exists(image)) result = image;
                }
            }
            return !(string.IsNullOrEmpty((string)result));
        }

        public bool ConvertBack(object val, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            throw new NotImplementedException();
        }
    }

    public class LanguageConverter : IValueConverter
    {
        public bool Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            result = null;
            if (value == null) return false;
            string lang = value.ToString();
            string filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Skin\default\images\LanguageFlags\" + lang + ".png");
            if (System.IO.File.Exists(filename))
            {
                result = filename;
                return true;
            }
            return false;
        }

        public bool ConvertBack(object val, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            throw new NotImplementedException();
        }
    }

    public class NullVisibilityConverter : IValueConverter
    {
        public bool Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            result = string.IsNullOrEmpty(value as string);
            if (parameter != null) result = !(bool)result;
            return true;
        }

        public bool ConvertBack(object val, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            throw new NotImplementedException();
        }
    }

    public class ExtendedInfoConverter : IValueConverter
    {
        public bool Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            result = string.Empty;
            IVideoDetails vd = value as IVideoDetails;
            string param = parameter as string;
            if (vd != null && !string.IsNullOrEmpty(param))
            {
                string myValue;
                if (vd.GetExtendedProperties().TryGetValue(param, out myValue)) 
                {
                    result = myValue;
                    return true;
                }
            }
            return false;
        }

        public bool ConvertBack(object val, Type targetType, object parameter, System.Globalization.CultureInfo culture, out object result)
        {
            throw new NotImplementedException();
        }
    }

}