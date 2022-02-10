using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Exceptions;

using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RevitToRDFConverter
{
    class App : IExternalApplication
    {
        const string RIBBON_TAB = "RDF";
        const string RIBBON_PANEL = "Converters";
        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            //Get the ribbon tab
            try { application.CreateRibbonTab(RIBBON_TAB); }
            catch (Exception){ } //Tab already exists

            //Get or create the panel
            RibbonPanel panel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(RIBBON_TAB);
            foreach (RibbonPanel x in panels)
            {
                if(x.Name == RIBBON_PANEL)
                {
                    panel = x;
                    break;
                }
            }

            // Couldn't find the panel, create it
            if (panel == null)
            {
                panel = application.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL);
            }
            // Get the image for the button
            Image img = Properties.Resources.Mikki;
            ImageSource imgSrc = GetImageSource(img);

            //Create the image for the button
            PushButtonData btnData = new PushButtonData(
                "BOT",
                "BOT",
                Assembly.GetExecutingAssembly().Location, "RevitToRDFConverter.Command"
                )
            {
                ToolTip = "Short Description that is shown when you hover over the button",
                LongDescription = "Longer description shown when hover over button for a few seconds",
                Image = imgSrc,
                LargeImage = imgSrc
            };

            //Add the button to the ribbon
            PushButton button = panel.AddItem(btnData) as PushButton;
            button.Enabled = true;

            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private BitmapSource GetImageSource(Image img)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                ms.Position = 0;
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }
    }
}
