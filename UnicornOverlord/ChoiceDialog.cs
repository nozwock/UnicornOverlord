using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace UnicornOverlord;


public partial class ChoiceDialog : Window
{
	public object SelectedItem => ViewModel.SelectedItem;

	private dynamic ViewModel => DataContext;

	private ChoiceDialog()
	{
		InitializeComponent();
	}

	public static bool Show<T>(
		IEnumerable<T> items,
		Func<T, string> keySelector,
		out T? selectedItem,
		IEnumerable<(string name, bool isChecked, Predicate<T> predicate)>? filters = null,
		Window? owner = null)
	{
		var vm = new ChoiceDialogViewModel<T>(items, keySelector, filters);

		var dialog = new ChoiceDialog
		{
			Owner = owner ?? Application.Current.Windows
				.OfType<Window>()
				.FirstOrDefault(w => w.IsActive),
			DataContext = vm
		};

		if (dialog.ShowDialog() == true)
		{
			selectedItem = vm.SelectedValue;
			return true;
		}

		selectedItem = default;
		return false;
	}

	private void Ok_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}

public sealed class ChoiceDialogViewModel<T> : ObservableObject
{
	public sealed class ChoiceItem<T1>(T1 value, Func<T1, string> keySelector)
	{
		public T1 Value { get; } = value;
		public string Display { get; } = keySelector(value);
	}

	public sealed class CheckboxFilter<T2>(
		string name,
		Predicate<T2> predicate,
		bool isChecked = false) : ObservableObject
	{
		public string Name { get; } = name;
		public Predicate<T2> Predicate { get; } = predicate;

		public bool IsChecked
		{
			get;
			set => SetProperty(ref field, value);
		} = isChecked;
	}

	public readonly Func<T, string> _keySelector;

	public ObservableCollection<ChoiceItem<T>> Items { get; }
	public ICollectionView ItemsView { get; }

	public ObservableCollection<CheckboxFilter<T>> Filters { get; }

	public string FilterText
	{
		get;
		set
		{
			if (SetProperty(ref field, value))
				ItemsView.Refresh();
		}
	} = "";

	public ChoiceItem<T>? SelectedItem
	{
		get;
		set
		{
			if (SetProperty(ref field, value))
				OnPropertyChanged(nameof(CanConfirm));
		}
	}

	public T? SelectedValue => SelectedItem == null ? default : SelectedItem.Value;

	public bool CanConfirm => SelectedItem != null;

	public ChoiceDialogViewModel(
		IEnumerable<T> items,
		Func<T, string> keySelector,
		IEnumerable<(string name, bool isChecked, Predicate<T> predicate)>? filters = null)
	{
		_keySelector = keySelector;

		Items = new ObservableCollection<ChoiceItem<T>>(items.Select(it => new ChoiceItem<T>(it, _keySelector)) ?? []);
		Filters = new ObservableCollection<CheckboxFilter<T>>(
			filters?.Select(f => new CheckboxFilter<T>(f.name, f.predicate, f.isChecked))
			?? []
		);

		ItemsView = CollectionViewSource.GetDefaultView(Items);
		ItemsView.Filter = FilterItem;

		foreach (var filter in Filters)
		{
			filter.PropertyChanged += (_, __) => ItemsView.Refresh();
		}
	}

	private bool FilterItem(object obj)
	{
		if (obj is not ChoiceItem<T> item)
			return false;

		// Search bar
		if (!string.IsNullOrWhiteSpace(FilterText))
		{
			var key = _keySelector(item.Value);
			if (key == null || !key.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
				return false;
		}

		// Checkbox filters
		foreach (var filter in Filters.Where(f => f.IsChecked))
		{
			if (!filter.Predicate(item.Value))
				return false;
		}

		return true;
	}
}
