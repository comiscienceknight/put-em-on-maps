﻿

#pragma checksum "F:\Project\MyWorld\Surface Maps\Surface Maps\Pages\MapView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5BFFB1850459358E98467E49DE163734"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Surface_Maps.Pages
{
    partial class MapView : global::Surface_Maps.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 21 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GoBackClick;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 31 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_Localize_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 57 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_AddAPushPin_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 70 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_MoveSelectedPushPin_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 84 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_DisplayHidePushpin_Click;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 110 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.map_Tapped;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 128 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_AddSingleFile_Click;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 132 "..\..\..\Pages\MapView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_AddPhotoVideoAlbum_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


