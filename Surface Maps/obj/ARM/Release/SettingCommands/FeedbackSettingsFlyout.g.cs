﻿

#pragma checksum "F:\Project\MyWorld\Surface Maps 20121214\Surface Maps\Surface Maps\SettingCommands\FeedbackSettingsFlyout.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "21EE5F54A48217792A9F1B4233683520"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Surface_Maps.SettingCommands
{
    partial class FeedbackSettingsFlyout : global::Surface_Maps.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 137 "..\..\..\SettingCommands\FeedbackSettingsFlyout.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.TextBlock_Tapped_1;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 138 "..\..\..\SettingCommands\FeedbackSettingsFlyout.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.Image_Tapped;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 127 "..\..\..\SettingCommands\FeedbackSettingsFlyout.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.MySettingsBackClicked;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


