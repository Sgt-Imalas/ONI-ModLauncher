﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace ONIModLauncher
{
	/// <summary>
	/// Interaction logic for ModListControl.xaml
	/// </summary>
	public partial class ModListControl : UserControl
	{
		private bool warningAcknowledged = false;

		private readonly DelayedEvent searchTypingDelay = new DelayedEvent(TimeSpan.FromMilliseconds(200));

		public ModListControl()
		{
			InitializeComponent();
		}

		internal void SetupFilters()
		{
			modsList.ItemsSource = ModManager.Instance.Mods;
			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(modsList.ItemsSource);
			view.Filter = FilterMethod;
			view.Refresh();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			searchTypingDelay.DelayFinished += SearchTypingDelay_DelayFinished;

			Launcher.Instance.PropertyChanged += Launcher_PropertyChanged;

			modListLockScreen.Visibility = Visibility.Collapsed;
		}

		private void Launcher_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Launcher_PropertyChanged(sender, e));
				return;
			}

			GongSolutions.Wpf.DragDrop.DragDrop.SetIsDragSource(modsList, Launcher.Instance.IsNotRunning);
			modListLockScreen.Visibility = (warningAcknowledged || Launcher.Instance.IsNotRunning) ? Visibility.Collapsed : Visibility.Visible;
			saveModListButton.IsEnabled = Launcher.Instance.IsNotRunning;
			loadModListButton.IsEnabled = Launcher.Instance.IsNotRunning;
			selectAllModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			unselectAllModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			sortModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			bisectTopButton.IsEnabled = Launcher.Instance.IsNotRunning;
			bisectBottomButton.IsEnabled = Launcher.Instance.IsNotRunning;
		}

		private void refreshModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.LoadModList();

			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(modsList.ItemsSource);
			view.Filter = FilterMethod;
			view.Refresh();
		}

		private void selectAllModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.EnableAllMods();
		}

		private void unselectAllModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.DisableAllMods();
		}

		private void sortModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.SortMods();
		}

		private void saveModListButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == true)
			{
				ModManager.Instance.SaveModList(dlg.FileName);
			}
		}

		private void loadModListButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (dlg.ShowDialog() == true)
			{
				ModManager.Instance.LoadModList(dlg.FileName);
			}
		}

		private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			searchTypingDelay.Start();
		}

		private void ModTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			searchTypingDelay.Start();
		}

		private void SearchTypingDelay_DelayFinished(object sender, EventArgs e)
		{
			FilterModsList();
		}

		private void FilterModsList()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(FilterModsList);
				return;
			}

			if (modsList != null)
			{
				CollectionViewSource.GetDefaultView(modsList.ItemsSource)?.Refresh();
			}
		}

		private bool FilterMethod(object item)
		{
			ONIMod mod = (ONIMod)item;

			int folderTypeIndex = modTypeComboBox.SelectedIndex;
			if (folderTypeIndex != 0)
			{
				if (folderTypeIndex == 1 && !mod.IsDev) return false;
				if (folderTypeIndex == 2 && !mod.IsLocal) return false;
				if (folderTypeIndex == 3 && !mod.IsSteam) return false;
			}

			string searchText = searchBox.Text;
			if (string.IsNullOrEmpty(searchText)) return true;

			bool titleMatched = mod.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
			//bool authorMatched = mod.Author?.ToLowerInvariant().Contains(searchText.ToLowerInvariant()) ?? false;

			return titleMatched;
		}

		private void ToggleKeepEnabledMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				mod.KeepEnabled = !mod.KeepEnabled;
			}
		}

		private void ToggleBrokenMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				mod.IsBroken = !mod.IsBroken;
			}
		}
		private void TogglePendingMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				mod.HasTogglePending = !mod.HasTogglePending;
			}
		}

		private void MoveToTopMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				ModManager.Instance.Mods.Remove(mod);
				ModManager.Instance.Mods.Insert(0, mod);
			}
		}

		private void MoveToBottomMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				ModManager.Instance.Mods.Remove(mod);
				ModManager.Instance.Mods.Add(mod);
			}
		}

		private void BisectTopButton_OnClick(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.BisectTop();
		}

		private void BisectBottomButton_OnClick(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.BisectBottom();
		}

		private void HideGameRunningWarningBtn_Click(object sender, RoutedEventArgs e)
		{
			warningAcknowledged = true;
			modListLockScreen.Visibility = Visibility.Collapsed;
		}
	}
}
