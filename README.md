# ListDiff

[![Build and Test](https://github.com/praeclarum/ListDiff/actions/workflows/build.yml/badge.svg)](https://github.com/praeclarum/ListDiff/actions/workflows/build.yml) [![NuGet Package](https://img.shields.io/nuget/v/ListDiff.svg)](https://www.nuget.org/packages/ListDiff)

Calculate a diff between any two lists and optionally merge them to aid in synchronizing data.

## Installation

```bash
dotnet add package ListDiff
```

## Diffing

A `ListDiff` object creates diff actions when it is constructed.

```csharp
using ListDiff;

var diff = new ListDiff<char, char> ("abc", "ac");

Console.WriteLine (diff);
```


## Merging

Merging is a way to change a given list so that it
mirrors another list. This is useful for updating
data-bound control lists to reflect new data.

For example:

```csharp
// This list is data-bound to the UI
ObservableCollection<Person> DisplayList; 

// This function fetches new data
Task<List<Person>> GetNewListAsync () {}

// Here's how to update the UI
async Task UpdateUI ()
{
    var newList = await GetNewListAsync ();

    DisplayList.MergeInto (newList, (x, y) => x.Id == y.Id);
}
```

The `UpdateUI` will call `Insert` and `RemoveAt` on
`DisplayList` to make sure the UI is showing the
latest data with the minimal amount of changes.
