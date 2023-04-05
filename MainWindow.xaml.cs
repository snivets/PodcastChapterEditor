// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ATL;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ChapEdit
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private ObservableCollection<ChapterInfo> ChapterItems;

		public MainWindow() {
			this.InitializeComponent();
			this.ExtendsContentIntoTitleBar = true;
			SetTitleBar(AppTitleBar);

			ChapterItems = new ObservableCollection<ChapterInfo>();
		}

		private async void PickAFileButton_Click(object sender, RoutedEventArgs e) {
			// Clear previous returned file name, if it exists, between iterations of this scenario
			PickAFileButton.Content = "Select an audio file with chapters";
			//TimestampDisplayTextbox.Blocks.Clear();

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
			if (file != null) {
				PickAFileButton.Content = file.Name;
				var audioMeta = new AudioTagParser(file);
				ChapterItems.Clear();
				var chaps = audioMeta.GetChapters().ToList();
				chaps.ForEach(c => ChapterItems.Add(c));
			} else {
				PickAFileButton.Content = "Select an audio file with chapters";
			}
		}

		private void TimestampUpdated(object sender, TextChangedEventArgs e) {
			// Get the current text of the TextBox
			var text = ((TextBox)sender).Text;

			// Use a regular expression to only allow numeric values
			var regex = new Regex("[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,3})?");
			var millisRegex = new Regex("[0-9]+");

			// If the text does not match the regular expression, undo the change
			if (!regex.IsMatch(text)) {
				if (millisRegex.IsMatch(text)) {
					((TextBox)sender).Text = FormatChapterTime(UInt32.Parse(text));
				} else {
					((TextBox)sender).Undo();
				}
			}
		}

		public static string FormatChapterTime(UInt32 millis) {
			var chapterStart = TimeSpan.FromMilliseconds(millis);
			return $"{chapterStart.Hours:00}:{chapterStart.Minutes:00}:{chapterStart.Seconds:00}.{chapterStart.Milliseconds:00}";
		}
	}
}
