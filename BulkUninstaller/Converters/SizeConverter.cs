using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BulkUninstaller.Converters
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is long size)
            {
                if (size < 1024) return size.ToString() + " B";

                
                if(size / 1024 > 0)// KB
                {
                    if(size / 1024 / 1024 > 0)// MB
                    {
                        if (size / 1024 / 1024 / 1024 > 0)// GB
                        {
                            return FormattedValue(((double)(size / 1024 / 1024)) / 1024) + " GB";
                        }
                        return FormattedValue(((double)(size / 1024)) / 1024) + " MB";
                    }
                    return FormattedValue((double)(size / 1024)) + " KB";
                }
            }
            return value;
        }

        private object FormattedValue(double value)
        {
            return string.Format("{0:N2}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
