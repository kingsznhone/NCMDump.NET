# NCMDump.NET

Decipher .ncm file to mp3 || flac

## Feature

Highly Optimized Corelib.

Keep all ID3 tags and cover image. Very nice.

## Changelog

### v1.3

Bug fix.

Async optimization.

### v1.2

Bug fix.

Drag directory || Select folder on GUI support.

### v1.1

Drag directory on executable support.

Bug fix

## Usage

### CLI

Drag .ncm file or directory on exe

### GUI

Don't ask, Just use.

## System Requirement

### OS : Windows 10 10.0.19041.0 with .NET6 Runtime

## API

### NCMDump.Convert(String) Method

Convert NCM file into MP3/FLAC format in synchronous.

```
public bool Convert(string path)
```
#### Parameters

```path``` String

File Path to a NCM file.


### NCMDump.ConvertAsync(String) Method

Convert NCM file into MP3/FLAC format in asynchronous.

```
public async Task<bool> ConvertAsync(string path)
```
#### Parameters

```path``` String

File Path to a NCM file.


## Refrence

<https://github.com/anonymous5l/ncmdump>
