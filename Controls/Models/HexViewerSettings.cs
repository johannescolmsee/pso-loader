using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Controls.Models
{
    public class HexViewerSettings : NotifyPropertyChangedBase
    {
        private ObservableCollection<Typeface> _FixedWidthFonts;
        public ObservableCollection<Typeface> FixedWidthFonts
        {
            get
            {
                return _FixedWidthFonts;
            }
            set
            {
                if (_FixedWidthFonts != value)
                {
                    _FixedWidthFonts = value;
                    OnPropertyChanged(nameof(FixedWidthFonts));
                }
            }
        }

        private Typeface _DisplayFont;
        public Typeface DisplayFont
        {
            get
            {
                return _DisplayFont;
            }
            set
            {
                if (_DisplayFont != value)
                {
                    _DisplayFont = value;
                    OnPropertyChanged(nameof(DisplayFont));
                }
            }
        }

        private double _FontSize = 12.0;
        public double FontSize
        {
            get
            {
                return _FontSize;
            }
            set
            {
                if (_FontSize != value)
                {
                    _FontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }




        private HexViewerSettings()
        {
            var fixedWidthTypefaces = Fonts.SystemTypefaces
                .GroupBy(x => x.FontFamily.ToString())
                .Select(grp => grp.First())
                .Where(x =>
                    new FormattedText("Hl1ajK", CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, x, 10, Brushes.Black).Width
                    ==
                    new FormattedText("HHOA1u", CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, x, 10, Brushes.Black).Width);

            this.FixedWidthFonts = new ObservableCollection<Typeface>(fixedWidthTypefaces);

            this.DisplayFont = this.FixedWidthFonts.LastOrDefault();
        }

        private static HexViewerSettings _Instance;
        public static HexViewerSettings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new HexViewerSettings()
                    {

                    };
                }
                return _Instance;
            }
        }
    }
}
