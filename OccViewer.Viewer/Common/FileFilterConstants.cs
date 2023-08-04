using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.DataFormats;

namespace OccViewer.Viewer.Common
{
    public class FileFilterConstants
    {
        public const string BrepFilter = "BREP Files (*.brep *.rle)|*.brep; *.rle";
        public const string StepFilter = "STEP Files (*.stp *.step)|*.stp; *.step";
        public const string IgesFilter = "IGES Files (*.igs *.iges)|*.igs; *.iges";
        public const string VRMLFilter = "VRML Files (*.wrl *.vrml)|*.wrl; *.vrml";
        public const string STLFilter = "STL Files (*.stl)|*.stl";
        public const string BMPFilter = "BMP Files (*.bmp)|*.bmp";
        public const string AllFilter = "All files (*.*)|*.*";
        public readonly static string[] ImportFilterArray = new string[] 
        { 
            IgesFilter, 
            StepFilter, 
            BrepFilter, 
            AllFilter 
        };
        public readonly static string ImportFilterString = string.Join("|", ImportFilterArray);
        public readonly static ModelFormat[] ImportFormatArray = new ModelFormat[] 
        { 
            ModelFormat.IGES, 
            ModelFormat.STEP, 
            ModelFormat.BREP 
        };

        public readonly static string[] ExportFilterArray = new string[]
        {
            BrepFilter, 
            StepFilter, 
            IgesFilter, 
            VRMLFilter, 
            STLFilter, 
            BMPFilter
        };
        public readonly static string ExportFilterString = string.Join("|", ExportFilterArray);
        public readonly static ModelFormat[] ExportFormatArray = new ModelFormat[]
        {
            ModelFormat.BREP, 
            ModelFormat.STEP, 
            ModelFormat.IGES, 
            ModelFormat.VRML, 
            ModelFormat.STL, 
            ModelFormat.BMP
        };
        /// <summary>
        /// GetModelFormatByExtension (file extension with dot(.))
        /// </summary>
        /// <param name="ext">file extension with dot(.)</param>
        /// <returns></returns>
        public static ModelFormat GetModelFormatByExtension(string ext)
        {
            ModelFormat format = ext switch
            {
                ".brep" => ModelFormat.BREP,
                ".rle" => ModelFormat.BREP,
                ".stp" => ModelFormat.STEP,
                ".step" => ModelFormat.STEP,
                ".igs" => ModelFormat.IGES,
                ".iges" => ModelFormat.IGES,
                ".stl" => ModelFormat.STL,
                ".wrl" => ModelFormat.VRML,
                ".vrml" => ModelFormat.VRML,
                ".bmp" => ModelFormat.BMP,
                _ => ModelFormat.Unknown
            };
            return format;
        }

    }
}
