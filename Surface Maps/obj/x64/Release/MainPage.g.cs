﻿

#pragma checksum "F:\Project\MyWorld\Surface Maps\Surface Maps\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "12C6D9F670FD2705D4B154ABBCB9B36C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Surface_Maps
{
    partial class MainPage : global::Surface_Maps.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 14 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Closed += this.AppBar_TopAppBar_Closed;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 17 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_RemoveLifeMap_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 27 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_ChangeLifeMapName_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 37 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_ChangeLifeMapBackground_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 47 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_EnterSelectedLifeMap_Click;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 64 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.AppBar)(target)).Closed += this.AppBar_BottomAppBar_Closed;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 68 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_UpdateLiftMapName_Click;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 72 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Button_AddNewLifeMap_Click;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 110 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.LifeMapGrid_Tapped;
                 #line default
                 #line hidden
                break;
            case 10:
                #line 160 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.itemGridView_SelectionChanged;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

