﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Media.Core;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;

namespace XamlBrewer.Uwp.ItemsRepeater.Sample
{
    public sealed partial class MainPage : Page
    {
        private List<Genre> _genres = new List<Genre>();
        private Compositor _compositor = Window.Current.Compositor;
        private SpringVector3NaturalMotionAnimation _springAnimation;

        public List<Genre> Genres => _genres;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            string xml;

            using (var client = new HttpClient())
            {
                xml = await client.GetStringAsync("http://trailers.apple.com/trailers/home/xml/current.xml");
            }

            var movies = XDocument.Parse(xml);

            var genreNames = movies.XPathSelectElements("//genre/name")
                          .Select(m => m.Value)
                          .OrderBy(m => m)
                          .Distinct()
                          .ToList();

            foreach (var genreName in genreNames)
            {
                _genres.Add(new Genre()
                {
                    Name = genreName,
                    Movies = movies.XPathSelectElements("//genre[name='" + genreName + "']")
                        .Ancestors("movieinfo")
                        .Select(m => new Movie()
                        {
                            Title = m.XPathSelectElement("info/title").Value,
                            PosterUrl = m.XPathSelectElement("poster/xlarge").Value,
                            TrailerUrl = m.XPathSelectElement("preview/large").Value
                        })
                        //.OrderBy(m => m.Title)
                        .ToList()
                });
                _genres.Add(new Genre()
                {
                    Name = genreName,
                    Movies = movies.XPathSelectElements("//genre[name='" + genreName + "']")
                        .Ancestors("movieinfo")
                        .Select(m => new Movie()
                        {
                            Title = m.XPathSelectElement("info/title").Value,
                            PosterUrl = m.XPathSelectElement("poster/xlarge").Value,
                            TrailerUrl = m.XPathSelectElement("preview/large").Value
                        })
                        //.OrderBy(m => m.Title)
                        .ToList()
                });
                _genres.Add(new Genre()
                {
                    Name = genreName,
                    Movies = movies.XPathSelectElements("//genre[name='" + genreName + "']")
                        .Ancestors("movieinfo")
                        .Select(m => new Movie()
                        {
                            Title = m.XPathSelectElement("info/title").Value,
                            PosterUrl = m.XPathSelectElement("poster/xlarge").Value,
                            TrailerUrl = m.XPathSelectElement("preview/large").Value
                        })
                        //.OrderBy(m => m.Title)
                        .ToList()
                });
            }

            GenreRepeater.ItemsSource = Genres;

        }

        private void Movie_Click(object sender, RoutedEventArgs e)
        {
            FlyoutShowOptions options = new FlyoutShowOptions();
            options.ShowMode = FlyoutShowMode.Standard;
            options.Placement = FlyoutPlacementMode.Top;

            MovieCommands.ShowAt(sender as FrameworkElement, options);
        }

        private async void Element_Click(object sender, RoutedEventArgs e)
        {
            // It stays on top of the dialog.
            MovieCommands.Hide();

            var movie = (sender as FrameworkElement)?.DataContext as Movie;
            var source = MediaSource.CreateFromUri(new Uri(movie.TrailerUrl));

            TitleText.Text = movie.Title;
            Player.Source = source;
            await MediaPlayerDialog.ShowAsync();
        }

        private void MediaPlayerDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Prevent the player to continue playing.
            Player.Source = null;
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = _compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void Element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Scale up a little.
            CreateOrUpdateSpringAnimation(1.02f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void Element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Scale back down.
            CreateOrUpdateSpringAnimation(1.0f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void ItemsRepeater_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Just a sample to show the idea - will need to account for key up
            // and also finding the correct item to put focus on.

            // find the next genre below and set focus.
            if(e.Key == Windows.System.VirtualKey.Down)
            {
                var current = e.OriginalSource as FrameworkElement;
                while( current.Parent != null && current.Parent != sender)
                {
                    current = (FrameworkElement)current.Parent;
                }

                var repeater = sender as Microsoft.UI.Xaml.Controls.ItemsRepeater;
                var index = repeater.GetElementIndex(current);

                if(index < repeater.ItemsSourceView.Count -1)
                {
                    var element = repeater.GetOrCreateElement(index + 1);

                    // You likely want to find a better one than the first focusible element.
                    var focusible = FocusManager.FindFirstFocusableElement(element) as Control;
                    focusible.Focus(FocusState.Keyboard);
                    e.Handled = true; // Dont bubble up so XYFocus logic does not try to handle this key press.
                }
            }
        }
    }
}
