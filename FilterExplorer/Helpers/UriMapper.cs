using System;
using System.Windows.Navigation;

namespace MusicLens.Helpers
{
    class UriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            Uri mappedUri = uri;

            if (uri.ToString().Contains("ViewfinderLaunch"))
            {
                mappedUri = new Uri("/Pages/PhotoPage.xaml?ViewfinderLaunch", UriKind.Relative);
            }

            if ((uri.ToString().Contains("EditPhotoContent")) && (uri.ToString().Contains("FileId")))
            {
                string tempUri = uri.ToString();
                string newUri= uri.ToString();
                mappedUri = new Uri(newUri.Replace("StreamPage", "PhotoPage"), UriKind.Relative);
                return mappedUri;
            }

            return mappedUri;
        }
    }
}