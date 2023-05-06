// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ATL;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ChapEdit
{
	/// <summary>
	/// THe one, the only WinUI 3 window for Chapter Editor.
	/// Not the most exotic application architecture around!
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private ObservableCollection<FormattedAudioChapter> Chapters;
		private ChapterEditorAudioLayer Audio;
		private AppWindow appWindow;

		public MainWindow() {
			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;
			SetTitleBar(AppTitleBar);

			this.Chapters = new ObservableCollection<FormattedAudioChapter>();

			IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this); // m_window in App.cs
			WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
			appWindow = AppWindow.GetFromWindowId(windowId);

			// Set title and icon
			appWindow.Title = "Audio Chapter Editor";
			appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Resources\\bostonbeansound-ico.ico"));

			ResizeWindowToContents();
		}

		private void ResizeWindowToContents() {
			var size = new SizeInt32();
			if (Chapters.Count == 0 && !AlbumArtViewPane.IsPaneOpen) {
				size.Width = 400;
				size.Height = 400;
			} else {
				size.Width = 700;
				size.Height = 700;
			}
			appWindow.Resize(size);
		}

		/// <summary>
		/// The ATL library uses a much more expansive object called ChapterInfo
		/// </summary>
		private ChapterInfo[] ConvertChapterFormat() {
			var atlChapters = new List<ChapterInfo>();
			Chapters.ToList().ForEach(c => atlChapters.Add(c.GetChapterInfo()));
			return atlChapters.ToArray();
		}

		private async void PickFile(object sender, RoutedEventArgs e) {
			// Create a file picker
			var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

			// Retrieve the window handle (HWND) of the current WinUI 3 window.
			var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

			// Initialize the file picker with the window handle (HWND).
			WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

			// Set options for your file picker
			openPicker.ViewMode = PickerViewMode.Thumbnail;
			openPicker.FileTypeFilter.Add(".mp3");
			openPicker.FileTypeFilter.Add(".m4a");

			// Open the picker for the user to pick a file
			var file = await openPicker.PickSingleFileAsync();

			// Album art updating
			if (file != null) {
				PickAFileButton.Content = file.Name;
				this.Audio = new ChapterEditorAudioLayer(file);
				Chapters.Clear();
				var chaps = Audio.GetChapters().ToList();
				FileInfoPanel.Visibility = Visibility.Visible;
				AddButton.Visibility = Visibility.Visible;
				Scroller.Visibility = Visibility.Visible;
				ArtButton.Visibility = Visibility.Visible;
				SaveButton.Visibility = Visibility.Collapsed;
				if (Audio.GetAlbumArt() != null) {
					var bitmapImage = new BitmapImage();
					bitmapImage.CreateOptions = BitmapCreateOptions.None;
					using (var stream = new InMemoryRandomAccessStream()) {
						using (var writer = new DataWriter(stream)) {
							writer.WriteBytes(Audio.GetAlbumArt());
							await writer.StoreAsync();
							await writer.FlushAsync();
							writer.DetachStream();
						}
						stream.Seek(0);
						await bitmapImage.SetSourceAsync(stream);
					}
					AlbumArt.Source = bitmapImage;
				}
				else {
					AlbumArt.Source = null;
					FileInfoPanel.Text = string.Empty;
				}
				AlbumArtViewPane.IsPaneOpen = Audio.GetAlbumArt() != null;

				// Chapter info updating - the good stuff
				if (chaps.Any()) {
					chaps.ForEach(c => Chapters.Add(
						new FormattedAudioChapter(c.Title,
							ChapterEditorAudioLayer.FormatChapterTimeFromMillis(c.StartTime),
							Chapters.Count)));
					FileInfoPanel.Text = Audio.GetFileInfo();
				}
				Scroller.Visibility = chaps.Any() ? Visibility.Visible : Visibility.Collapsed;
				NoChaptersInfoText.Visibility = chaps.Any() ? Visibility.Collapsed : Visibility.Visible;
			}

			ResizeWindowToContents();
		}

		// Identical to the focus method for Chapter Title, but we don't want to select all text there when focused
		// (we don't assume edits to this field will involve replacing the entire contents).
		private void TimestampFocused(object sender, RoutedEventArgs e) {
			CheckSaveButton();
			var textBox = (TextBox)sender;
			textBox.Select(0, textBox.Text.Length);
		}

		private void ChapterTitleFocused(object sender, RoutedEventArgs e) {
			CheckSaveButton();
		}

		private void TimestampLeft(object sender, RoutedEventArgs e) {
			// Get the current text of the TextBox
			var timeStr = ((TextBox)sender).Text;
			// Assess if it's a valid timestamp
			var parseResult = ChapterEditorAudioLayer.GetTimestampString(timeStr);

			// Move this into a method so it can be called from FormattedTimestamp
			if (parseResult.SuccessfullyParsed && TimeSpan.Parse(parseResult.TimestampResult).TotalSeconds <= Audio.Duration) {
				((TextBox)sender).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
				((TextBox)sender).Text = parseResult.TimestampResult;
			} else {
				((TextBox)sender).Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 0));
				// Set this here since a call to CheckSaveButton() only checks the object array, which isn't updated until after this
				SaveButton.IsEnabled = false;
			}
		}

		private void TimestampChanged(object sender, TextChangedEventArgs e) {
			((TextBox)sender).Text = Regex.Match(((TextBox)sender).Text, @"[:\.0-9]*").Value;
		}

		private void AddNewChapter(object sender, RoutedEventArgs e) {
			Chapters.Add(new FormattedAudioChapter(Chapters.Count));
			Scroller.Visibility = Visibility.Visible;
			NoChaptersInfoText.Visibility = Visibility.Collapsed;
		}

		private void DeleteChapter(object sender, RoutedEventArgs e) {
			var index = (int)((Button)sender).Tag;
			Chapters.Remove(Chapters.First(c => c.Index == index));
		}

		private void MoveChapterUp(object sender, RoutedEventArgs e) {
			var index = (int)((Button)sender).Tag;
			// Since chapters are not guaranteed to be sequential, modifying the Index by -1 can result in wonky behavior.
			// Instead, splice the entire chapters object and rearrange it.
			var position = Chapters.IndexOf(Chapters.First(c => c.Index == index));
			var chapter = Chapters[position];
			if (position != 0) {
				Chapters.RemoveAt(position);
				Chapters.Insert(position-1, chapter);
			}
		}

		private void CheckSaveButton() {
			if (SaveButton.Visibility == Visibility.Collapsed)
				SaveButton.Visibility = Visibility.Visible;
			if (!Chapters.ToList().Any(c => !ChapterEditorAudioLayer.GetTimestampString(c.Timestamp).SuccessfullyParsed))
				SaveButton.IsEnabled = true;
		}

		private void Save(object sender, RoutedEventArgs e) {
			Audio.UpdateChapters(ConvertChapterFormat());
		}

		private async void ChangeArt(object sender, RoutedEventArgs e) {
			// Same logic as above
			var openPicker = new FileOpenPicker();
			var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
			openPicker.ViewMode = PickerViewMode.Thumbnail;

			openPicker.FileTypeFilter.Add(".jpg");
			openPicker.FileTypeFilter.Add(".jpeg");
			openPicker.FileTypeFilter.Add(".png");

			var file = await openPicker.PickSingleFileAsync();

			// Add to audio file
			if (file != null) {
				PickAFileButton.Content = file.Name;
				var image = await file.OpenReadAsync();

				// Set art frame (since above method works differently for file load)
				var bitmap = new BitmapImage();
				bitmap.SetSource(image);
				AlbumArt.Source = bitmap;
				AlbumArtViewPane.IsPaneOpen = true;

				// Create byte array for ATL
				byte[] imgBytes = new byte[image.Size];
				await image.ReadAsync(imgBytes.AsBuffer(), (uint)image.Size, InputStreamOptions.None);
				Audio.UpdateAlbumArt(imgBytes);

				// Make sure the Save button is now visible
				CheckSaveButton();
			}

			ResizeWindowToContents();
		}
	}
}
