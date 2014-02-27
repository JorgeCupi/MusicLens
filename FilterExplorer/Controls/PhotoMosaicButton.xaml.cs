using MusicLens.ViewModels;
using System.Windows.Controls;

namespace MusicLens.Controls
{
    public partial class PhotoMosaicButton : UserControl
    {
        public PhotoMosaicButton()
        {
            InitializeComponent();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhotoMosaicButtonViewModel m = DataContext as PhotoMosaicButtonViewModel;

            if (m != null)
            {
                m.RaiseRetrieveRequest();
            }
        }
    }
}
