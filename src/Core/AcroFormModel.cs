using System;
using System.Collections.Generic;

namespace GinPdfRw.Core
{
    public enum FormFieldType
    {
        Text,
        CheckBox,
        RadioButton,
        ComboBox
    }

    public class FormFieldItem
    {
        public string Name { get; set; } = string.Empty;
        public FormFieldType Type { get; set; } = FormFieldType.Text;
        public int PageIndex { get; set; } = 0;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string TextValue { get; set; } = string.Empty;
        public bool IsChecked { get; set; } = false;
        public bool IsCustom { get; set; } = false; // Added via Click & Type / Fill & Sign
    }

    public class EditorAnnotationItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int PageIndex { get; set; } = 0;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "Text"; // "Text", "Checkmark", "Image", "Signature"
        public string ImagePath { get; set; } = string.Empty;
        public double FontSize { get; set; } = 14;
        public string ColorHex { get; set; } = "#000000";
    }
}
