﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

/// <summary>
///     Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole
///     list is refreshed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    ///     Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class.
    /// </summary>
    public ObservableRangeCollection()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains
    ///     elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">collection: The collection from which the elements are copied.</param>
    /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception>
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    ///     Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
    /// </summary>
    public void AddRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        foreach (var i in collection) Items.Add(i);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Removes the first occurence of each item in the specified collection from ObservableCollection(Of T).
    /// </summary>
    public void RemoveRange(int _start, int _count)
    {
        Debug.Assert(_start >= 0);
        Debug.Assert(_count >= 0);
        Debug.Assert(_start + _count <= Items.Count);

        var limit = Math.Min(Items.Count, _start + _count);
        for (var i = limit - 1; i >= _start; i--)
            Items.RemoveAt(i);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Removes the first occurence of each item in the specified collection from ObservableCollection(Of T).
    /// </summary>
    public void RemoveRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        foreach (var i in collection) Items.Remove(i);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Clears the current collection and replaces it with the specified item.
    /// </summary>
    public void Replace(T item)
    {
        ReplaceRange(new[] {item});
    }

    /// <summary>
    ///     Clears the current collection and replaces it with the specified collection.
    /// </summary>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));

        Items.Clear();
        foreach (var i in collection) Items.Add(i);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public T Find(Predicate<T> _predicate)
    {
        var list = this.ToList();
        return list.Find(_predicate);
    }
}