using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace MyWX
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // show the StatusBar
            ShowStatusBar();
        }

        // show the StatusBar
        private async void ShowStatusBar()
        {
            // turn on SystemTray for mobile
            // don't forget to add a Reference to Windows Mobile Extensions For The UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var StatusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await StatusBar.ShowAsync();
                StatusBar.ForegroundColor = Windows.UI.Colors.White;
            }
        }

        // MyPivot 的前一个索引
        int PreIndex = 0;

        private void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 将变换前的 Header 变“暗”
            switch (PreIndex)
            {
                case 0:
                    MainFrame.Visibility = Visibility.Visible;
                    MainFrameHL.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    Contacts.Visibility = Visibility.Visible;
                    ContactsHL.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Discover.Visibility = Visibility.Visible;
                    DiscoverHL.Visibility = Visibility.Collapsed;
                    break;
            }
            // 当前的 Index 将会变成“切换前”的 Index 
            PreIndex = (sender as Pivot).SelectedIndex;

            // 将当前的 Header 变“亮”
            switch ((sender as Pivot).SelectedIndex)
            {
                case 0:
                    MainFrame.Visibility = Visibility.Collapsed;
                    MainFrameHL.Visibility = Visibility.Visible;
                    MFHL_Img.Opacity = 1.0;
                    break;
                case 1:
                    Contacts.Visibility = Visibility.Collapsed;
                    ContactsHL.Visibility = Visibility.Visible;
                    ContactsHL_Img.Opacity = 1.0;
                    break;
                case 2:
                    Discover.Visibility = Visibility.Collapsed;
                    DiscoverHL.Visibility = Visibility.Visible;
                    DiscoverHL_Img.Opacity = 1.0;
                    break;
            }
        }

        private void TappedPivot0(object sender, TappedRoutedEventArgs e)
        {
            MyPivot.SelectedIndex = 0;
        }
        private void TappedPivot1(object sender, TappedRoutedEventArgs e)
        {
            MyPivot.SelectedIndex = 1;
        }
        private void TappedPivot2(object sender, TappedRoutedEventArgs e)
        {
            MyPivot.SelectedIndex = 2;
        }


        private void New_Search(object sender, RoutedEventArgs e)
        {

        }

        // Debug 数据输出,左侧面板 TranslateX 的变化
        private void mySplit_Loaded(object sender, RoutedEventArgs e)
        {
            #if DEBUG

            Grid grid = Utility.FindVisualChild<Grid>(mySplit);

            Binding bind = new Binding();
            bind.Path = new PropertyPath("TranslateX");
            bind.Source = (grid.FindName("PaneRoot") as Grid).RenderTransform as CompositeTransform;
            
            #endif
        }

        #region  从屏幕左侧边缘滑动屏幕时，打开 SplitView 菜单

        // SplitView 控件模板中，Pane部分的 Grid
        Grid PaneRoot;

        //  引用 SplitView 控件中， 保存从 Pane “关闭” 到“打开”的 VisualTransition
        //  也就是 <VisualTransition From="Closed" To="OpenOverlayLeft"> 这个 
        VisualTransition from_ClosedToOpenOverlayLeft_Transition;

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (mySplit.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                if (PaneRoot == null)
                {
                    // 找到 SplitView 控件中，模板的父容器
                    Grid grid = Utility.FindVisualChild<Grid>(mySplit);

                    PaneRoot = grid.FindName("PaneRoot") as Grid;

                    if (from_ClosedToOpenOverlayLeft_Transition == null)
                    {
                        // 获取 SplitView 模板中“视觉状态集合”
                        IList<VisualStateGroup> stateGroup = VisualStateManager.GetVisualStateGroups(grid);

                        //  获取 VisualTransition 对象的集合。
                        IList<VisualTransition> transitions = stateGroup[0].Transitions;

                        // 找到 SplitView.IsPaneOpen 设置为 true 时，播放的 transition
                        from_ClosedToOpenOverlayLeft_Transition = transitions?.Where(train => train.From == "Closed" && train.To == "OpenOverlayLeft").First();

                        // 遍历所有 transitions，打印到输出窗口
                        foreach (var tran in transitions)
                        {
                            Debug.WriteLine("From : " + tran.From + "   To : " + tran.To);
                        }
                    }
                }


                // 默认为 Collapsed，所以先显示它
                PaneRoot.Visibility = Visibility.Visible;

                // 当在 Border 上向右滑动，并且滑动的总距离需要小于 Panel 的默认宽度。否则会脱离左侧窗口，继续向右拖动
                if (e.Cumulative.Translation.X >= 0 && e.Cumulative.Translation.X < mySplit.OpenPaneLength)
                {
                    CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;
                    ct.TranslateX = (e.Cumulative.Translation.X - mySplit.OpenPaneLength);
                }

            }
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;

            // 仅当 SplitView 处于 Overlay 模式时（窗口宽度最小时）
            if (mySplit.DisplayMode == SplitViewDisplayMode.Overlay && PaneRoot != null)
            {
                // 因为当 IsPaneOpen 为 true 时，会通过 VisualStateManager 把 PaneRoot.Visibility  设置为
                // Visibility.Visible，所以这里把它改为 Visibility.Collapsed，以回到初始状态
                PaneRoot.Visibility = Visibility.Collapsed;

                // 恢复初始状态 
                CompositeTransform ct = PaneRoot.RenderTransform as CompositeTransform;


                // 如果大于 MySplitView.OpenPaneLength 宽度的 1/2 ，则显示，否则隐藏
                if ((mySplit.OpenPaneLength + ct.TranslateX) > mySplit.OpenPaneLength / 2)
                {
                    mySplit.IsPaneOpen = true;
                    // mySplit.IsPaneOpen = true;

                    // 因为上面设置 IsPaneOpen = true 会再次播放向右滑动的动画，所以这里使用 SkipToFill()
                    // 方法，直接跳到动画结束状态
                    from_ClosedToOpenOverlayLeft_Transition?.Storyboard?.SkipToFill();
                }

                ct.TranslateX = 0;
            }

        }
        #endregion

    }
}
